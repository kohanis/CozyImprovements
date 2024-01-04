using HarmonyLib;
using UnityEngine;

namespace SpyciBot.LC.CozyImprovements.Improvements;

[HarmonyPatch]
public static class Accessibility
{
    private static HangarShipDoor hangarShipDoor;

    // 
    // Launch Lever Fixes
    // - Make hitbox of launch lever huge so it's easy to pull
    //
    [HarmonyPatch(typeof(StartMatchLever), "Start")]
    [HarmonyPostfix]
    private static void Postfix_StartMatchLever_Start(StartMatchLever __instance)
    {
        // Don't bother if the config option is disabled
        if (!CozyImprovements.CozyConfig.configEasyLaunchLever.Value)
            return;

        // Make the lever wide and flat, making it easy to press anywhere on the main section of the desk
        __instance.transform.localScale = new Vector3(1.139f, 0.339f, 1.539f);
        __instance.transform.localPosition = new Vector3(8.7938f, 1.479f, -7.0767f);

        // reset the playerPos so the lever pull animation is normal
        __instance.transform.GetChild(0).position = new Vector3(8.8353f, 0.2931f, -14.5767f);
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
        if (!CozyImprovements.CozyConfig.configBigDoorButtons.Value)
            return;

        hangarShipDoor = __instance;
        var buttonPanel = __instance.hydraulicsDisplay.transform.parent.gameObject;

        // Make the whole panel bigger and centered on the 2 beams
        buttonPanel.transform.localScale = new Vector3(-2f, -2f, -2f);
        buttonPanel.transform.localPosition = new Vector3(-5.2085f, 1.8882f, -8.823f);

        // Adjust the size of the Start Button collider to match the Stop button
        var startButton = buttonPanel.transform.Find("StartButton").gameObject;
        var stopButton = buttonPanel.transform.Find("StopButton").gameObject;

        stopButton.transform.localScale = new Vector3(-1.1986f, -0.1986f, -1.1986f);
        startButton.transform.localScale = stopButton.transform.localScale;

        startButton.transform.GetChild(0).localPosition = stopButton.transform.GetChild(0).localPosition;
        startButton.transform.GetChild(0).localScale = stopButton.transform.GetChild(0).localScale;


        // Fix Emissives of buttons
        var startButtonMats = startButton.GetComponent<MeshRenderer>().materials;
        for (var i = 0; i < startButtonMats.Length; i++)
        {
            if (startButtonMats[i].name == "GreenButton (Instance)")
            {
                startButtonMats[i].SetColor("_EmissiveColor", new Color32(39, 51, 39, 255));
            }

            if (startButtonMats[i].name == "ButtonWhite (Instance)")
            {
                startButtonMats[i].SetColor("_EmissiveColor", new Color32(179, 179, 179, 255));
            }
        }

        startButton.GetComponent<MeshRenderer>().materials = startButtonMats;

        var stopButtonMats = stopButton.GetComponent<MeshRenderer>().materials;
        for (var i = 0; i < stopButtonMats.Length; i++)
        {
            if (stopButtonMats[i].name == "RedButton (Instance)")
            {
                stopButtonMats[i].SetColor("_EmissiveColor", new Color32(64, 24, 24, 255));
            }

            if (stopButtonMats[i].name == "ButtonWhite (Instance)")
            {
                stopButtonMats[i].SetColor("_EmissiveColor", new Color32(179, 179, 179, 255));
            }
        }

        stopButton.GetComponent<MeshRenderer>().materials = stopButtonMats;


        // Make Buttons Interact Area Bigger

        var startButtonInteract = buttonPanel.transform.Find("StartButton").GetChild(0);
        var stopButtonInteract = buttonPanel.transform.Find("StopButton").GetChild(0);


        //StartButtonInteract.GetComponent<MeshRenderer>().enabled = true;
        startButtonInteract.GetComponent<MeshRenderer>().material.color = new Color32(39, 255, 39, 255);

        //StopButtonInteract.GetComponent<MeshRenderer>().enabled = true;
        stopButtonInteract.GetComponent<MeshRenderer>().material.color = new Color32(255, 24, 24, 255);


        var pressablePosition = new Vector3(-3.7205f, 2.0504f, -16.3018f);
        var pressableScale = new Vector3(0.7393f, 0.4526f, 0.6202f);
        var notPressableScale = new Vector3(0.003493f, 0.000526f, 0.002202f);

        startButtonInteract.position = pressablePosition;
        startButtonInteract.localScale = pressableScale;
        stopButtonInteract.position = pressablePosition;
        stopButtonInteract.localScale = notPressableScale;
    }

    //
    // Hangar Door Button Panel Fixes
    // - Toggle which button is usable depending on if the door is open or not
    //
    [HarmonyPatch(typeof(StartOfRound), "SetShipDoorsClosed")]
    [HarmonyPostfix]
    private static void Postfix_StartOfRound_SetShipDoorsClosed(StartOfRound __instance, bool closed)
    {
        // Don't bother if the config option is disabled
        if (!CozyImprovements.CozyConfig.configBigDoorButtons.Value)
            return;

        var buttonPanel = hangarShipDoor.hydraulicsDisplay.transform.parent.gameObject;
        var startButtonInteract = buttonPanel.transform.Find("StartButton").GetChild(0);
        var stopButtonInteract = buttonPanel.transform.Find("StopButton").GetChild(0);

        var pressableScale = new Vector3(0.7393f, 0.4526f, 0.6202f);
        var notPressableScale = new Vector3(0.003493f, 0.000526f, 0.002202f);

        startButtonInteract.localScale = pressableScale;
        stopButtonInteract.localScale = pressableScale;

        if (closed)
        {
            //StartButtonInteract.GetComponent<MeshRenderer>().enabled = true;
            startButtonInteract.localScale = pressableScale;

            //StopButtonInteract.GetComponent<MeshRenderer>().enabled = false;
            stopButtonInteract.localScale = notPressableScale;
        }
        else
        {
            //StopButtonInteract.GetComponent<MeshRenderer>().enabled = true;
            stopButtonInteract.localScale = pressableScale;

            //StartButtonInteract.GetComponent<MeshRenderer>().enabled = false;
            startButtonInteract.localScale = notPressableScale;
        }
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
        if (!CozyImprovements.CozyConfig.configBigTeleporterButtons.Value)
            return;
        var teleporterButton = __instance.buttonTrigger.gameObject.transform.parent.gameObject;
        teleporterButton.transform.localScale = (Vector3.one * 3f);
    }

    //
    // Monitor Fixes
    // - Make the Monitor Buttons bigger
    //
    public static void AdjustMonitorButtons(GameObject buttonCube)
    {
        // Don't bother if the config option is disabled
        if (!CozyImprovements.CozyConfig.configBigMonitorButtons.Value)
            return;
        var button = buttonCube.transform.parent.gameObject;
        button.transform.localScale = new Vector3(1.852f, 1.8475f, 1.852f);

        if (button.name == "CameraMonitorSwitchButton")
        {
            button.transform.localPosition = new Vector3(-0.28f, -1.807f, -0.29f);
        }
    }
}