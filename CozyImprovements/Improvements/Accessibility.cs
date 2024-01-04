using HarmonyLib;
using UnityEngine;

namespace SpyciBot.LC.CozyImprovements.Improvements;

[HarmonyPatch]
public static class Accessibility
{
    // 
    // Launch Lever Fixes
    // - Make hitbox of launch lever huge so it's easy to pull
    //
    [HarmonyPatch(typeof(StartMatchLever), "Start")]
    [HarmonyPostfix]
    private static void Postfix_StartMatchLever_Start(StartMatchLever __instance)
    {
        // Don't bother if the config option is disabled
        if (!Configs.EasyLaunchLever.Value)
            return;

        var levelTransform = __instance.transform;

        // Make the lever wide and flat, making it easy to press anywhere on the main section of the desk
        levelTransform.localScale = new Vector3(1.139f, 0.339f, 1.539f);
        levelTransform.localPosition = new Vector3(8.7938f, 1.479f, -7.0767f);

        // reset the playerPos so the lever pull animation is normal
        levelTransform.Find("playerPos").position = new Vector3(8.8353f, 0.2931f, -14.5767f);
    }

    // 
    // Hangar Door Button Panel Fixes
    // - This makes the panel much bigger and makes it easy to press button to open/close the doors
    //
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPostfix]
    private static void Postfix_HangarShipDoor_Start(HangarShipDoor __instance)
    {
        // Don't bother if the config option is disabled
        if (!Configs.BigDoorButtons.Value)
            return;

        var buttonPanel = __instance.hydraulicsDisplay.transform.parent;

        // Make the whole panel bigger and centered on the 2 beams
        buttonPanel.localScale = new Vector3(-2f, -2f, -2f);
        buttonPanel.localPosition = new Vector3(-5.2085f, 1.8882f, -8.823f);

        // Adjust the size of the Start Button collider to match the Stop button
        var startButton = buttonPanel.Find("StartButton");
        var stopButton = buttonPanel.Find("StopButton");

        var newLocalScale = new Vector3(-1.1986f, -0.1986f, -1.1986f);
        stopButton.localScale = newLocalScale;
        startButton.localScale = newLocalScale;

        // Make Buttons Interact Area Bigger

        var startButtonInteract = startButton.GetChild(0);
        var stopButtonInteract = stopButton.GetChild(0);

        var pressablePosition = new Vector3(-3.7205f, 2.0504f, -16.3018f);
        var pressableScale = new Vector3(0.7393f, 0.4526f, 0.6202f);

        startButtonInteract.GetComponent<MeshRenderer>().material.color = new Color32(39, 255, 39, 255);
        startButtonInteract.position = pressablePosition;
        startButtonInteract.localScale = pressableScale;

        stopButtonInteract.GetComponent<MeshRenderer>().material.color = new Color32(255, 24, 24, 255);
        stopButtonInteract.position = pressablePosition;
        stopButtonInteract.localScale = pressableScale;

        stopButtonInteract.GetComponent<BoxCollider>().enabled = false;

        // Fix Emissives of buttons

        var startButtonMaterials = startButton.GetComponent<MeshRenderer>().materials;
        foreach (var material in startButtonMaterials)
        {
            switch (material.name)
            {
                case "GreenButton (Instance)":
                    material.SetColor(ShaderIDs.EmissiveColor.Value, new Color32(39, 51, 39, 255));
                    break;
                case "ButtonWhite (Instance)":
                    material.SetColor(ShaderIDs.EmissiveColor.Value, new Color32(179, 179, 179, 255));
                    break;
            }
        }

        startButton.GetComponent<MeshRenderer>().materials = startButtonMaterials;

        var stopButtonMaterials = stopButton.GetComponent<MeshRenderer>().materials;
        foreach (var material in stopButtonMaterials)
        {
            switch (material.name)
            {
                case "RedButton (Instance)":
                    material.SetColor(ShaderIDs.EmissiveColor.Value, new Color32(64, 24, 24, 255));
                    break;
                case "ButtonWhite (Instance)":
                    material.SetColor(ShaderIDs.EmissiveColor.Value, new Color32(179, 179, 179, 255));
                    break;
            }
        }

        stopButton.GetComponent<MeshRenderer>().materials = stopButtonMaterials;
    }

    //
    // Hangar Door Button Panel Fixes
    // - Toggle which button is usable depending on if the door is open or not
    //
    [HarmonyPatch(typeof(HangarShipDoor), "SetDoorClosed")]
    [HarmonyPostfix]
    private static void Postfix_HangarShipDoor_SetDoorClosed(HangarShipDoor __instance) =>
        OnDoorStateChanged(__instance, true);

    //
    // Hangar Door Button Panel Fixes
    // - Toggle which button is usable depending on if the door is open or not
    //
    [HarmonyPatch(typeof(HangarShipDoor), "SetDoorOpen")]
    [HarmonyPostfix]
    private static void Postfix_HangarShipDoor_SetDoorOpen(HangarShipDoor __instance) =>
        OnDoorStateChanged(__instance, false);

    private static void OnDoorStateChanged(HangarShipDoor door, bool closed)
    {
        // Don't bother if the config option is disabled
        if (!Configs.BigDoorButtons.Value)
            return;

        var buttonPanel = door.hydraulicsDisplay.transform.parent;

        buttonPanel.Find("StartButton").GetChild(0).GetComponent<BoxCollider>().enabled = closed;
        buttonPanel.Find("StopButton").GetChild(0).GetComponent<BoxCollider>().enabled = !closed;
    }

    //
    // Teleporter Fixes
    // - Make the Teleporter Buttons bigger
    //
    [HarmonyPatch(typeof(ShipTeleporter), "Awake")]
    [HarmonyPostfix]
    private static void Postfix_ShipTeleporter_Awake(ShipTeleporter __instance)
    {
        // Don't bother if the config option is disabled
        if (!Configs.BigTeleporterButtons.Value)
            return;

        var teleporterButton = __instance.buttonTrigger.transform.parent;
        teleporterButton.localScale = Vector3.one * 3f;
    }

    //
    // Monitor Fixes
    // - Make the Monitor Buttons bigger
    //
    public static void AdjustMonitorButtons(GameObject buttonCube)
    {
        // Don't bother if the config option is disabled
        if (!Configs.BigMonitorButtons.Value)
            return;

        var button = buttonCube.transform.parent;
        button.localScale = new Vector3(1.852f, 1.8475f, 1.852f);

        if (button.gameObject.name == "CameraMonitorSwitchButton")
            button.localPosition = new Vector3(-0.28f, -1.807f, -0.29f);
    }
}