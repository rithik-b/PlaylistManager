using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using IPA.Utilities;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistViewButtonsController : IInitializable, IDisposable, ILevelCollectionUpdater, ILevelCategoryUpdater, ILevelCollectionsTableUpdater
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private readonly PlaylistDetailsViewController playlistDetailsViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        private int downloadingBeatmapCollectionIdx;
        private IAnnotatedBeatmapLevelCollection[] downloadingBeatmapLevelCollections;
        private CancellationTokenSource tokenSource;
        private Playlist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;

        public event Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;

        [UIComponent("root")]
        private readonly Transform rootTransform;

        [UIComponent("sync-button")]
        private readonly Transform syncButtonTransform;

        public PlaylistViewButtonsController(LevelPackDetailViewController levelPackDetailViewController, PopupModalsController popupModalsController, 
            PlaylistDetailsViewController playlistDetailsViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;
            this.playlistDetailsViewController = playlistDetailsViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;

            tokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            syncButtonTransform.transform.localScale *= 0.08f;
            syncButtonTransform.gameObject.SetActive(false);
            rootTransform.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        #region Details

        [UIAction("details-click")]
        private void ShowDetails()
        {
            playlistDetailsViewController.ShowDetails();
        }


        #endregion

        #region Delete

        [UIAction("delete-click")]
        private void OnDelete()
        {
            popupModalsController.ShowYesNoModal(rootTransform, string.Format("Are you sure you would like to delete {0}?", selectedPlaylist.Title), DeleteButtonPressed);
        }

        private void DeleteButtonPressed()
        {
            try
            {
                parentManager.DeletePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                int selectedIndex = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
                List<IAnnotatedBeatmapLevelCollection> annotatedBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToList();
                annotatedBeatmapLevelCollections.RemoveAt(selectedIndex);
                selectedIndex--;
                LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections.ToArray(), selectedIndex < 0 ? 0 : selectedIndex);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(rootTransform, "Error: Playlist cannot be deleted.", null);
                Plugin.Log.Critical(string.Format("An exception was thrown while deleting a playlist.\nException message:{0}", e));
            }
        }

        #endregion

        #region Download

        [UIAction("download-click")]
        private async Task DownloadPlaylistAsync()
        {
            popupModalsController.ShowOkModal(rootTransform, "", CancelButtonPressed, "Cancel");

            List<IPlaylistSong> missingSongs;
            if (selectedPlaylist is BlistPlaylist blistPlaylist)
            {
                missingSongs = blistPlaylist.Where(s => s.PreviewBeatmapLevel == null).Select(s => s).ToList();
            }
            else if (selectedPlaylist is LegacyPlaylist legacyPlaylist)
            {
                missingSongs = legacyPlaylist.Where(s => s.PreviewBeatmapLevel == null).Select(s => s).ToList();
            }
            else
            {
                popupModalsController.OkText = "Error: The selected playlist cannot be downloaded.";
                popupModalsController.OkButtonText = "Ok";
                return;
            }

            popupModalsController.OkText = string.Format("{0}/{1} songs downloaded", 0, missingSongs.Count);
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            for (int i = 0; i < missingSongs.Count; i++)
            {
                if (!string.IsNullOrEmpty(missingSongs[i].Key))
                {
                    await DownloaderUtils.instance.BeatmapDownloadByKey(missingSongs[i].Key.ToLower(), tokenSource.Token);
                }
                else if (!string.IsNullOrEmpty(missingSongs[i].Hash))
                {
                    await DownloaderUtils.instance.BeatmapDownloadByHash(missingSongs[i].Hash, tokenSource.Token);
                }
                popupModalsController.OkText = string.Format("{0}/{1} songs downloaded", i + 1, missingSongs.Count);
            }

            popupModalsController.OkText = "Download Complete!";
            popupModalsController.OkButtonText = "Ok";
            downloadingBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToArray();
            downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            SongCore.Loader.Instance.RefreshSongs(false);
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        private void CancelButtonPressed()
        {
            tokenSource.Cancel();
        }

        private void LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent()
        {
            LevelCollectionTableViewUpdatedEvent?.Invoke(downloadingBeatmapLevelCollections, downloadingBeatmapCollectionIdx);
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        #endregion

        #region Sync

        [UIAction("sync-click")]
        private async Task SyncPlaylistAsync()
        {
            object outSyncURL;

            if (!selectedPlaylist.TryGetCustomData("syncURL", out outSyncURL))
            {
                popupModalsController.ShowOkModal(rootTransform, "Error: The selected playlist cannot be synced", null);
                return;
            }

            string syncURL = (string)outSyncURL;

            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            popupModalsController.ShowOkModal(rootTransform, "Syncing Playlist", CancelButtonPressed, "Cancel");
            Stream playlistStream = null;

            try
            {
                playlistStream = new MemoryStream(await DownloaderUtils.instance.DownloadFileToBytesAsync(syncURL, tokenSource.Token));
                ((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist).Clear(); // Clear all songs
                PlaylistLibUtils.playlistManager.DefaultHandler.Populate(playlistStream, (BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException))
                {
                    popupModalsController.OkText = "Error: The selected playlist cannot be synced";
                    popupModalsController.OkButtonText = "Ok";
                }
                return;
            }
            finally
            {
                // If the downloaded playlist doesn't have the sync url, add it back
                if (!selectedPlaylist.TryGetCustomData("syncURL", out outSyncURL))
                {
                    selectedPlaylist.SetCustomData("syncURL", syncURL);
                }

                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                await DownloadPlaylistAsync();
                popupModalsController.ShowOkModal(rootTransform, "Playlist Synced", null);
            }
        }

        #endregion

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection selectedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (selectedBeatmapLevelCollection is Playlist selectedPlaylist)
            {
                this.selectedPlaylist = selectedPlaylist;
                this.parentManager = parentManager;

                rootTransform.gameObject.SetActive(true);
                if (selectedPlaylist.TryGetCustomData("syncURL", out _))
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

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory, bool _)
        {
            if (levelCategory != SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                rootTransform.gameObject.SetActive(false);
            }
        }
    }
}
