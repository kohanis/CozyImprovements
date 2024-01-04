using BepInEx.Configuration;

namespace SpyciBot.LC.CozyImprovements;

public static class Configs
{
    // General
    public static ConfigEntry<bool> StorageLights;
    public static ConfigEntry<bool> LightSwitchGlow;
    public static ConfigEntry<bool> TerminalGlow;
    public static ConfigEntry<bool> TerminalMonitorAlwaysOn;
    public static ConfigEntry<bool> ChargeStationGlow;

    // Accessibility
    public static ConfigEntry<bool> BigDoorButtons;
    public static ConfigEntry<bool> EasyLaunchLever;
    public static ConfigEntry<bool> BigTeleporterButtons;
    public static ConfigEntry<bool> BigMonitorButtons;

    internal static void Init(ConfigFile cfg)
    {
        // -------
        // General
        // -------
        StorageLights = cfg.Bind(
            "General",
            "StorageLightsEnabled",
            true,
            "Makes the LightSwitch glow in the dark"
        );
        LightSwitchGlow = cfg.Bind(
            "General",
            "LightSwitchGlowEnabled",
            true,
            "Makes the LightSwitch glow in the dark"
        );
        TerminalGlow = cfg.Bind(
            "General",
            "TerminalGlowEnabled",
            true,
            "Makes the Terminal glow active all the time"
        );
        TerminalMonitorAlwaysOn = cfg.Bind(
            "General",
            "TerminalMonitorAlwaysOn",
            true,
            "Makes the Terminal screen active all the time; Will show the screen you left it on"
        );
        ChargeStationGlow = cfg.Bind(
            "General",
            "ChargeStationGlowEnabled",
            true,
            "Makes the Charging Station glow with a yellow light"
        );

        // -------------
        // Accessibility
        // -------------
        BigDoorButtons = cfg.Bind(
            "General.Accessibility",
            "BigDoorButtonsEnabled",
            false,
            "Enlarges the door buttons so they're easier to press"
        );
        EasyLaunchLever = cfg.Bind(
            "General.Accessibility",
            "EasyLaunchLeverEnabled",
            true,
            "Enlarges the hitbox for the Launch Lever to cover more of the table so it's easier to pull"
        );
        BigTeleporterButtons = cfg.Bind(
            "General.Accessibility",
            "BigTeleporterButtonsEnabled",
            false,
            "Enlarges the teleporter buttons so they're easier to press"
        );
        BigMonitorButtons = cfg.Bind(
            "General.Accessibility",
            "BigMonitorButtonsEnabled",
            false,
            "Enlarges the Monitor buttons so they're easier to press"
        );
    }
}