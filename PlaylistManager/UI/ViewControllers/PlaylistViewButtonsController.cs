using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistViewButtonsController : IInitializable, ILevelCollectionUpdater
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private AnnotatedBeatmapLevelCollectionsTableView annotatedBeatmapLevelCollectionsTableView;
        private readonly PlaylistViewController playlistViewController;

        private Playlist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;

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
            try
            {
                parentManager.DeletePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);

                var annotatedBeatmapLevelCollections = AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsTableView).ToList();
                annotatedBeatmapLevelCollections.Remove((IAnnotatedBeatmapLevelCollection)selectedPlaylist);

                int selectedIndex = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, selectedIndex - 1, false);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollectionsTableView, annotatedBeatmapLevelCollections[selectedIndex - 1]);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(rootTransform, "Error: Playlist cannot be deleted.", null);
                Plugin.Log.Critical(string.Format("An exception was thrown while deleting a playlist.\nException message:{0}", e));
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

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection selectedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection is Playlist selectedPlaylist)
            {
                this.selectedPlaylist = selectedPlaylist;
                this.parentManager = parentManager;

                rootTransform.gameObject.SetActive(true);

                var customData = selectedPlaylist.CustomData;
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
                this.selectedPlaylist = null;
                this.parentManager = null;
                rootTransform.gameObject.SetActive(false);
            }
        }
    }
}
