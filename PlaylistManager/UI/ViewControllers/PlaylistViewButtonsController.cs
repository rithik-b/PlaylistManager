using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    internal class PlaylistViewButtonsController : IInitializable, ILevelCategoryUpdater
    {
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController;
        private readonly SettingsViewController settingsViewController;
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("queue-modal")]
        private readonly RectTransform queueModalTransform;

        private Vector3 queueModalPosition;

        public PlaylistViewButtonsController(PlaylistDownloaderViewController playlistDownloaderViewController, MainFlowCoordinator mainFlowCoordinator,
            SettingsViewController settingsViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.playlistDownloaderViewController = playlistDownloaderViewController;
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.settingsViewController = settingsViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), annotatedBeatmapLevelCollectionsViewController.gameObject, this);
        }

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory, bool viewControllerActivated)
        {
            if (rootTransform != null)
            {
                if (levelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
                {
                    rootTransform.gameObject.SetActive(true);
                }
                else
                {
                    rootTransform.gameObject.SetActive(false);
                }
            }
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            queueModalPosition = queueModalTransform.localPosition;
        }

        [UIAction("queue-click")]
        private void ShowQueue()
        {
            queueModalTransform.localPosition = queueModalPosition;
            playlistDownloaderViewController.SetParent(queueModalTransform, new Vector3(0.75f, 0.75f, 1f));
        }

        [UIAction("settings-click")]
        private void ShowSettings()
        {
            mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf().InvokeMethod<object, FlowCoordinator>("PresentViewController", new object[] { settingsViewController, null, ViewController.AnimationDirection.Vertical, false });
        }
    }
}
