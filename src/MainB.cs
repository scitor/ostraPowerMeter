#if !UMM
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;

namespace PowerMeter
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "space.scitor.Ostranauts.PowerMeter";
        public const string pluginName = "Power Meter";
        public const string pluginVersion = "0.5.0";

        ConfigEntry<bool> showInGUI = null!;
        ConfigEntry<bool> showInOverlay = null!;

        private void Awake()
        {
            showInGUI = Config.Bind("General", "showInGUI", true, "Show usage in Nav Display");
            showInGUI.SettingChanged += OnSettingChanged;

            showInOverlay = Config.Bind("General", "showInOverlay", true, "Show usage in Power Overlay");
            showInOverlay.SettingChanged += OnSettingChanged;

            OnSettingChanged(null!, null!);
            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(Patch).Assembly);

            Logger.LogInfo(pluginName + " v" + pluginVersion + " Ready");
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            Patch.showInGUI = showInGUI.Value;
            Patch.showInOverlay = showInOverlay.Value;
        }
    }
}
#endif
