using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistViewButtonsController : IInitializable, ILevelCollectionUpdater, ILevelCategoryUpdater, IRefreshable
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private AnnotatedBeatmapLevelCollectionsTableView annotatedBeatmapLevelCollectionsTableView;
        private readonly PlaylistViewController playlistViewController;

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.Accessor AnnotatedBeatmapLevelCollectionsTableViewAccessor =
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.GetAccessor("_annotatedBeatmapLevelCollectionsTableView");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsTableView, IReadOnlyList<IAnnotatedBeatmapLevelCollection>>.Accessor AnnotatedBeatmapLevelCollectionsAccessor =
            FieldAccessor<AnnotatedBeatmapLevelCollectionsTableView, IReadOnlyList<IAnnotatedBeatmapLevelCollection>>.GetAccessor("_annotatedBeatmapLevelCollections");

        [UIComponent("root")]
        private readonly Transform rootTransform;

        [UIComponent("sync-button")]
        private readonly Transform syncButtonTransform;

        public PlaylistViewButtonsController(LevelPackDetailViewController levelPackDetailViewController, PopupModalsController popupModalsController,
            AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, PlaylistViewController playlistViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            annotatedBeatmapLevelCollectionsTableView = AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController);
            this.playlistViewController = playlistViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            syncButtonTransform.transform.localScale *= 0.08f;
            syncButtonTransform.gameObject.SetActive(false);
            rootTransform.gameObject.SetActive(false);
        }

        #region Delete

        [UIAction("delete-click")]
        private void OnDelete()
        {
            popupModalsController.ShowYesNoModal(rootTransform, string.Format("Are you sure you would like to delete {0}?", annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection.collectionName), DeletePlaylist);
        }

        private void DeletePlaylist()
        {
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = (BeatSaberPlaylistsLib.Types.IPlaylist)annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            if (PlaylistLibUtils.playlistManager.GetManagerForPlaylist(selectedPlaylist).DeletePlaylist(selectedPlaylist))
            {
                var annotatedBeatmapLevelCollections = AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsTableView).ToList();
                annotatedBeatmapLevelCollections.Remove(selectedPlaylist);

                int selectedIndex = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, selectedIndex - 1, false);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollectionsTableView, annotatedBeatmapLevelCollections[selectedIndex - 1]);
            }
            else
            {
                popupModalsController.ShowOkModal(rootTransform, "Error: Playlist cannot be deleted.", null);
            }
        }

        #endregion

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
                rootTransform.gameObject.SetActive(true);
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
                rootTransform.gameObject.SetActive(false);
            }
        }

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            if (levelCategory != SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                rootTransform.gameObject.SetActive(false);
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
