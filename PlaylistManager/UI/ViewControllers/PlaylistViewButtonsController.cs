using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Interfaces;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    class PlaylistViewButtonsController : IInitializable, ILevelCollectionUpdater
    {
        private LevelPackDetailViewController levelPackDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private PlaylistViewController playlistViewController;

        [UIComponent("bg")]
        private Transform bgTransform;

        PlaylistViewButtonsController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, PlaylistViewController playlistViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.playlistViewController = playlistViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            bgTransform.gameObject.SetActive(false);
        }

        [UIAction("delete-click")]
        internal void OnDelete()
        {
            if(!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            playlistViewController.DisplayWarning();
        }

        [UIAction("download-click")]
        internal async System.Threading.Tasks.Task OnDownload()
        {
            if (!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            await playlistViewController.DownloadPlaylistAsync();
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection)
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && beatmapLevelCollection is Playlist)
            {
                bgTransform.gameObject.SetActive(true);
            }
            else
            {
                bgTransform.gameObject.SetActive(false);
            }
        }
    }
}
