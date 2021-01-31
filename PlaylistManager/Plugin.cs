using IPA;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;
using System.Reflection;
using SiraUtil.Zenject;
using PlaylistManager.Installers;
using PlaylistManager.Utilities;
using PlaylistManager.UI;
using BeatSaberMarkupLanguage.Settings;
using IPA.Config;
using IPA.Config.Stores;

namespace PlaylistManager
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        public const string HarmonyId = "com.github.rithik-b.PlaylistManager";
        internal static Harmony harmony;

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("PlaylistManager initialized.");
            harmony = new Harmony(HarmonyId);
            zenjector.OnMenu<PlaylistViewInstaller>();
            DownloaderUtils.Init();
        }

        #region BSIPA Config
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        #endregion

        [OnEnable]
        public void OnEnable()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            BSMLSettings.instance.AddSettingsMenu("PlaylistManager", "PlaylistManager.UI.Views.Settings.bsml", SettingsViewController.instance);
            RefreshButtonUI.instance.Setup();
        }

        [OnDisable]
        public void OnDisable()
        {
            harmony.UnpatchAll(HarmonyId);
            BSMLSettings.instance.RemoveSettingsMenu(SettingsViewController.instance);
            RefreshButtonUI.instance.Remove();
        }
    }
}
