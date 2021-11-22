using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    internal class PlaylistViewButtonsController : IInitializable
    {
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        [UIComponent("queue-modal")]
        private RectTransform queueModalTransform;

        public PlaylistViewButtonsController(PlaylistDownloaderViewController playlistDownloaderViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.playlistDownloaderViewController = playlistDownloaderViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), annotatedBeatmapLevelCollectionsViewController.gameObject, this);
        }

        [UIAction("queue-click")]
        private void ShowQueue()
        {
            playlistDownloaderViewController.SetParent(queueModalTransform, new Vector3(0.75f, 0.75f, 1f));
        }
    }
}
