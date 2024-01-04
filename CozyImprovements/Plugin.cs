using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using BepInEx;
using HarmonyLib;
using SpyciBot.LC.CozyImprovements.Improvements;
using Unity.Netcode;
using UnityEngine;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SpyciBot.LC.CozyImprovements;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[HarmonyPatch]
public class CozyImprovements : BaseUnityPlugin
{
    private static GameObject closetGameObject;
    private static Terminal termInst;

    public static Config CozyConfig { get; private set; }

    private void Awake()
    {
        // Plugin startup logic
        CozyConfig = new(Config);
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        Logger.LogInfo(
            $"Plugin {PluginInfo.PLUGIN_NAME} - {PluginInfo.PLUGIN_GUID} - {PluginInfo.PLUGIN_VERSION} is loaded!");
    }


    // 
    // Terminal Fixes
    //

    [HarmonyPatch(typeof(Terminal), "waitUntilFrameEndToSetActive")]
    [HarmonyPrefix]
    private static void Prefix_Terminal_WaitUntilFrameEndToSetActive(Terminal __instance, ref bool active)
    {
        termInst = __instance;
        if (CozyConfig.configTerminalMonitorAlwaysOn.Value)
        {
            // Force terminal canvas to always be turned on/visible
            active = true;
        }
    }

    [HarmonyPatch(typeof(Terminal), "SetTerminalInUseClientRpc")]
    [HarmonyPostfix]
    private static void Postfix_Terminal_SetTerminalInUseClientRpc(Terminal __instance, bool inUse)
    {
        if (CozyConfig.configTerminalGlow.Value)
        {
            // Force terminal light to always be turned on/visible
            termInst.terminalLight.enabled = true;
        }
    }


    //
    // Run on Client and Host
    //

    [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
    [HarmonyPostfix]
    private static void Postfix_StartOfRound_OnPlayerConnectedClientRpc(StartOfRound __instance, ulong clientId, int connectedPlayers, ulong[] connectedPlayerIdsOrdered, int assignedPlayerObjectId, int serverMoneyAmount, int levelID, int profitQuota, int timeUntilDeadline, int quotaFulfilled, int randomSeed)
    {
        // This will trigger on every client every time a client joins, so only do stuff if it's the joining client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            DoAllTheThings();
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "LoadUnlockables")]
    [HarmonyPostfix]
    private static void Postfix_StartOfRound_LoadUnlockables()
    {
        // This will only trigger on the host
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
        var array = GameObject.FindGameObjectsWithTag("InteractTrigger");
        for (var i = 0; i < array.Length; i++)
        {
            //Debug.Log($"{i} -- {array[i].name}");
            if (array[i].name == "LightSwitch")
            {
                if (CozyConfig.configLightSwitchGlow.Value)
                {
                    // Make the light switch panel glow green and make the switch glow red
                    MakeEmissive(array[i], new Color32(182, 240, 150, 102));
                    MakeEmissive(array[i].transform.GetChild(0).gameObject, new Color32(241, 80, 80, 10), 0.15f);
                }
            }

            if (array[i].name == "TerminalScript")
            {
                if (CozyConfig.configTerminalMonitorAlwaysOn.Value)
                {
                    //  Make terminal display the Store list on startup
                    termInst.LoadNewNode(termInst.terminalNodes.specialNodes[1]);
                }

                if (CozyConfig.configTerminalGlow.Value)
                {
                    // Force terminal light to always be turned on/visible
                    termInst.terminalLight.enabled = true;
                }
            }

            if (array[i].name == "Trigger")
            {
                if (CozyConfig.configChargeStationGlow.Value)
                {
                    var chargeStation = array[i].transform.parent.parent.gameObject;

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
                }
            }
            if (array[i].name == "Cube (2)" && array[i].transform.parent.gameObject.name.StartsWith("CameraMonitor"))
            {
                if (CozyConfig.configBigMonitorButtons.Value)
                {
                    Accessibility.AdjustMonitorButtons(array[i].gameObject);
                }
            }
        }
    }

    private static void MakeEmissive(GameObject gameObject, Color32 glowColor, float brightness = 0.02f)
    {
        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        var emMaterial = meshRenderer.material;

        emMaterial.SetColor("_EmissiveColor", (Color)glowColor * brightness);
        meshRenderer.material = emMaterial;
    }

    private static void ManageStorageCupboard()
    {
        var array = FindObjectsOfType<PlaceableShipObject>();
        for (var i = 0; i < array.Length; i++)
        {
            var sorInst = StartOfRound.Instance;
            var unlockableItem = sorInst.unlockablesList.unlockables[array[i].unlockableID];
            var unlockableType = unlockableItem.unlockableType;
            if (unlockableItem.unlockableName == "Cupboard")
            {
                /*
                Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Debug.Log("~~~~~~~~~~~~~~~~~~~~    LoadUnlockables    ~~~~~~~~~~~~~~~~~~~~~");
                Debug.Log(padString(unlockableItem.unlockableName, '~', 65));
                Debug.Log(padString("" + unlockableType, '~', 65));
                Debug.Log(padString("" + unlockableItem.spawnPrefab, '~', 65));
                Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                */
                closetGameObject = array[i].parentObject.gameObject;
                break;
            }
        }

        if (closetGameObject == null)
        {
            return;
        }


        // Don't bother if the config option is disabled
        if (CozyConfig.configStorageLights.Value)
        {
            SpawnStorageLights(closetGameObject);
        }
    }

    private static void SpawnStorageLights(GameObject storageCloset)
    {
        /*
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        Vector3 size = renderer.bounds.size;
        Debug.Log("Renderer bounds: " + size);
        */
        var midPoint = -1.1175f;

        var heightList = new List<float> { 2.804f, 2.163f, 1.48f, 0.999f };
        var lightOffset = 0.55f;

        // Top Shelf
        var shelfHeight = heightList[0];
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint - lightOffset, 0.4f, shelfHeight));
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint, 0.4f, shelfHeight));
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint + lightOffset, 0.4f, shelfHeight));
        // 2nd Shelf
        shelfHeight = heightList[1];
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint - lightOffset, 0.4f, shelfHeight));
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint, 0.4f, shelfHeight));
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint + lightOffset, 0.4f, shelfHeight));
        // 3rd Shelf
        shelfHeight = heightList[2];
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint - lightOffset, 0.4f, shelfHeight), 2.0f);
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint, 0.4f, shelfHeight), 2.0f);
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint + lightOffset, 0.4f, shelfHeight), 2.0f);
        // Bottom Shelf
        shelfHeight = heightList[3];
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint - lightOffset, 0.4f, shelfHeight));
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint, 0.4f, shelfHeight));
        AttachLightToStorageCloset(storageCloset, new Vector3(midPoint + lightOffset, 0.4f, shelfHeight));
    }
        
    private static void AttachLightToStorageCloset(GameObject storageCloset, Vector3 lightPositionOffset, float intensity = 3.0f)
    {
        // Create lightbulb object
        var lightObject = new GameObject("StorageClosetLight");
        var meshFilter = lightObject.AddComponent<MeshFilter>();
        var meshRenderer = lightObject.AddComponent<MeshRenderer>();

        var tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        meshFilter.mesh = tempSphere.GetComponent<MeshFilter>().mesh;

        // Make lightbulb glow
        var offWhiteMaterial = new Material(Shader.Find("HDRP/Lit"));
        Color lightSphereColor = new Color32(249, 240, 202, 255); // Off-white color
        offWhiteMaterial.SetColor("_BaseColor", lightSphereColor); // Set the base color (albedo)

        var emissiveIntensity = 1.0f;
        offWhiteMaterial.SetColor("_EmissiveColor", lightSphereColor * emissiveIntensity);
        meshRenderer.material = offWhiteMaterial;

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
        lightObject.transform.SetParent(storageCloset.transform, false);
    }


    //
    // Utilities
    //

    private static string PadString(string baseStr, char padChar, int width)
    {
        var paddingWidth = width - (baseStr.Length + 8);
        var padLeft = paddingWidth / 2 + (baseStr.Length + 8);
        var paddedStr = ("    " + baseStr + "    ").PadLeft(padLeft, padChar).PadRight(width, padChar);
        return paddedStr;
    }

    private static void ObviousDebug(string baseStr)
    {
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Debug.Log(PadString("" + baseStr, '~', 65));
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    }
}