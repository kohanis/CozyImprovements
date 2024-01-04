using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpyciBot.LC.CozyImprovements.Improvements;
using UnityEngine;

namespace SpyciBot.LC.CozyImprovements;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[HarmonyPatch]
public class CozyImprovements : BaseUnityPlugin
{
    internal static ManualLogSource Log;

    private void Awake()
    {
        // Plugin startup logic
        Log = Logger;
        Configs.Init(Config);
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        Logger.LogInfo(
            $"Plugin {PluginInfo.PLUGIN_NAME} - {PluginInfo.PLUGIN_GUID} - {PluginInfo.PLUGIN_VERSION} is loaded!");
    }


    // 
    // Terminal Fixes
    //
    
    [HarmonyPatch(typeof(Terminal), "Start")]
    [HarmonyPostfix]
    private static void Postfix_Terminal_Start(Terminal __instance)
    {
        if (Configs.TerminalMonitorAlwaysOn.Value)
        {
            //  Make terminal display the Store list on startup
            __instance.LoadNewNode(__instance.terminalNodes.specialNodes[1]);
        }

        if (Configs.TerminalGlow.Value)
        {
            // Force terminal light to always be turned on/visible
            __instance.terminalLight.enabled = true;
        }
    }

    [HarmonyPatch(typeof(Terminal), "waitUntilFrameEndToSetActive")]
    [HarmonyPrefix]
    private static void Prefix_Terminal_WaitUntilFrameEndToSetActive(ref bool active)
    {
        if (Configs.TerminalMonitorAlwaysOn.Value)
        {
            // Force terminal canvas to always be turned on/visible
            active = true;
        }
    }

    [HarmonyPatch(typeof(Terminal), "SetTerminalInUseClientRpc")]
    [HarmonyPostfix]
    private static void Postfix_Terminal_SetTerminalInUseClientRpc(Terminal __instance)
    {
        if (Configs.TerminalGlow.Value)
        {
            // Force terminal light to always be turned on/visible
            __instance.terminalLight.enabled = true;
        }
    }


    //
    // Run on Client and Host
    //

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPostfix]
    private static void Postfix_StartOfRound_Start()
    {
        DoAllTheThings();
    }


    //
    // All The Things™️
    //

    private static void DoAllTheThings()
    {
        ManageInteractables();
        ManageStorageCupboard();
    }

    private static void ManageInteractables()
    {
        // PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
        var interactTriggers = GameObject.FindGameObjectsWithTag("InteractTrigger");
        foreach (var trigger in interactTriggers)
        {
            switch (trigger.name)
            {
                //Log.LogInfo($" -- {trigger.name}");
                case "LightSwitch" when Configs.LightSwitchGlow.Value:
                {
                    // Make the light switch panel glow green and make the switch glow red
                    MakeEmissive(trigger, new Color32(182, 240, 150, 102));
                    MakeEmissive(
                        trigger.transform.Find("Switch").gameObject,
                        new Color32(241, 80, 80, 10),
                        0.15f
                    );

                    break;
                }
                case "Trigger" when Configs.ChargeStationGlow.Value:
                {
                    if (trigger.transform.parent is not { parent: { name: "ChargeStation" } chargeStation })
                        break;
                    
                    // Add a yellow glow to the ChargeStation
                    var lightObject = new GameObject("ChargeStationLight");
                    var lightComponent = lightObject.AddComponent<Light>();
                    lightComponent.type = LightType.Point;
                    lightComponent.color = new Color32(240, 240, 140, 255);
                    lightComponent.intensity = 0.05f;
                    lightComponent.range = 0.3f;
                    //lightComponent.spotAngle = 179.0f;
                    lightComponent.shadows = LightShadows.Soft;

                    lightObject.layer = LayerMask.NameToLayer("Room");
                    lightObject.transform.localPosition = new Vector3(0.5f, 0.0f, 0.0f);
                    //lightObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    lightObject.transform.SetParent(chargeStation.transform, false);

                    break;
                }
                case "Cube (2)" when Configs.BigMonitorButtons.Value:
                {
                    if (trigger.transform.parent.gameObject.name.StartsWith("CameraMonitor"))
                    {
                        Accessibility.AdjustMonitorButtons(trigger);
                    }

                    break;
                }
            }
        }
    }

    private static void MakeEmissive(GameObject gameObject, Color32 glowColor, float brightness = 0.02f)
    {
        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        var emMaterial = meshRenderer.material;

        emMaterial.SetColor(ShaderIDs.EmissiveColor.Value, (Color)glowColor * brightness);
        meshRenderer.material = emMaterial;
    }

    private static void ManageStorageCupboard()
    {
        // Don't bother if the config option is disabled
        if (!Configs.StorageLights.Value)
            return;

        var closetObject = GameObject.Find("Environment/HangarShip/StorageCloset");
        if (closetObject is null)
            return;

        SpawnStorageLights(closetObject);
    }

    private static void SpawnStorageLights(GameObject closetObject)
    {
        /*
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        Vector3 size = renderer.bounds.size;
        Log.LogInfo("Renderer bounds: " + size);
        */

        // Top Shelf
        AttachLightsToStorageCloset(closetObject, 2.804f);
        // 2nd Shelf
        AttachLightsToStorageCloset(closetObject, 2.163f);
        // 3rd Shelf
        AttachLightsToStorageCloset(closetObject, 1.48f, 2f);
        // Bottom Shelf
        AttachLightsToStorageCloset(closetObject, 0.999f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AttachLightsToStorageCloset(GameObject closetObject, float z, float intensity = 3f)
    {
        const float midPoint = -1.1175f;
        const float lightOffset = 0.55f;

        AttachLightToStorageCloset(closetObject, new Vector3(midPoint - lightOffset, 0.4f, z), intensity);
        AttachLightToStorageCloset(closetObject, new Vector3(midPoint, 0.4f, z), intensity);
        AttachLightToStorageCloset(closetObject, new Vector3(midPoint + lightOffset, 0.4f, z), intensity);
    }

    private static void AttachLightToStorageCloset(
        GameObject closetObject,
        Vector3 lightPositionOffset,
        float intensity = 3.0f)
    {
        // Use previous lightbulb object
        var lightObject = closetObject.transform.Find("StorageClosetLight")?.gameObject;
        if (lightObject is not null)
        {
            lightObject = Instantiate(lightObject, closetObject.transform, false);
            lightObject.GetComponent<Light>().intensity = intensity;
            lightObject.transform.localPosition = lightPositionOffset;
            return;
        }

        // Create lightbulb object
        lightObject = new GameObject("StorageClosetLight");
        var tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lightObject.AddComponent<MeshFilter>().mesh = tempSphere.GetComponent<MeshFilter>().mesh;

        // Make lightbulb glow
        const float emissiveIntensity = 1.0f;
        var offWhiteMaterial = new Material(Shader.Find("HDRP/Lit"));
        Color lightSphereColor = new Color32(249, 240, 202, 255); // Off-white color
        offWhiteMaterial.SetColor(ShaderIDs.BaseColor.Value, lightSphereColor); // Set the base color (albedo)
        offWhiteMaterial.SetColor(ShaderIDs.EmissiveColor.Value, lightSphereColor * emissiveIntensity);
        lightObject.AddComponent<MeshRenderer>().material = offWhiteMaterial;

        DestroyImmediate(tempSphere);

        // Add light beam from lightbulb
        var lightComponent = lightObject.AddComponent<Light>();
        lightComponent.type = LightType.Spot;
        lightComponent.color = lightSphereColor;
        lightComponent.intensity = intensity;
        lightComponent.range = 1.05f;
        lightComponent.spotAngle = 125.0f;
        lightComponent.shadows = LightShadows.Soft;

        lightObject.layer = LayerMask.NameToLayer("Room");
        lightObject.transform.localScale = new Vector3(0.125f, 0.125f, 0.04f);
        lightObject.transform.localPosition = lightPositionOffset;
        lightObject.transform.rotation = Quaternion.Euler(170, 0, 0);
        lightObject.transform.SetParent(closetObject.transform, false);
    }


    //
    // Utilities
    //

    private static string PadString(string baseStr, char padChar, int width)
    {
        var paddingWidth = width - baseStr.Length - 8;

        StringBuilder sb = new(width);
        sb.Append(padChar, paddingWidth / 2);
        sb.Append(' ', 4);
        sb.Append(baseStr);
        sb.Append(' ', 4);
        sb.Append(padChar, paddingWidth / 2 + paddingWidth % 2);

        return sb.ToString();
    }

    private static void ObviousDebug(string baseStr)
    {
        Log.LogInfo("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Log.LogInfo("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Log.LogInfo("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Log.LogInfo(PadString("" + baseStr, '~', 65));
        Log.LogInfo("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Log.LogInfo("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Log.LogInfo("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    }
}