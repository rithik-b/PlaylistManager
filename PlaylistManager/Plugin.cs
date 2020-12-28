using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using PlaylistManager.UI;
using HarmonyLib;
using System.Reflection;

namespace PlaylistManager
{
    [Plugin(RuntimeOptions.SingleStartInit)]
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
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("PlaylistManager initialized.");
            harmony = new Harmony(HarmonyId);
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            ApplyHarmonyPatches();
            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += BSEvents_menuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.levelSelected += BSEvents_levelSelected;
        }

        private void BSEvents_menuSceneLoadedFresh(ScenesTransitionSetupDataSO data)
        {
            AddPlaylistController.instance.Setup();
            PlaylistViewController.instance.Setup();
        }

        private void BSEvents_levelSelected(LevelCollectionViewController viewController, IPreviewBeatmapLevel beatmapLevel)
        {
            AddPlaylistController.instance.LevelSelected(beatmapLevel);
            PlaylistViewController.instance.LevelSelected(beatmapLevel);
        }

        public static void ApplyHarmonyPatches()
        {
            try
            {
                Log.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Log.Critical("Error applying Harmony patches: " + ex.Message);
                Log.Debug(ex);
            }
        }


        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

        }
    }
}
