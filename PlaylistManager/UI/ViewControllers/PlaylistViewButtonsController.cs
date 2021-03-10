using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Interfaces;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistViewButtonsController : IInitializable, ILevelCollectionUpdater, ILevelCategoryUpdater, IRefreshable
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly PlaylistViewController playlistViewController;

        [UIComponent("bg")]
        private readonly Transform bgTransform;

        [UIComponent("sync-button")]
        private readonly Transform syncButtonTransform;

        public PlaylistViewButtonsController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, PlaylistViewController playlistViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.playlistViewController = playlistViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            syncButtonTransform.transform.localScale *= 0.08f;
            syncButtonTransform.gameObject.SetActive(false);
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
        internal async Task OnDownload()
        {
            if (!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            await playlistViewController.DownloadPlaylistAsync();
        }

        [UIAction("sync-click")]
        internal async Task OnSync()
        {
            if (!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            await playlistViewController.SyncPlaylistAsync();
        }

        public void LevelCollectionUpdated()
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection is Playlist playlist)
            {
                bgTransform.gameObject.SetActive(true);
                var customData = playlist.CustomData;
                if (customData != null && customData.ContainsKey("syncURL"))
                {
                    syncButtonTransform.gameObject.SetActive(true);
                }
                else
                {
                    syncButtonTransform.gameObject.SetActive(false);
                }
            }
            else
            {
                bgTransform.gameObject.SetActive(false);
            }
        }

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            if (levelCategory != SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                bgTransform.gameObject.SetActive(false);
            }
            else
            {
                LevelCollectionUpdated();
            }
        }

        public void Refresh()
        {
            LevelCollectionUpdated();
        }
    }
}
