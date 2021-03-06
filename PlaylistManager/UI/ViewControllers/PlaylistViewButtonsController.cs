﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistViewButtonsController : IInitializable, IDisposable, INotifyPropertyChanged, ILevelCollectionUpdater, ILevelCategoryUpdater, ILevelCollectionsTableUpdater
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private readonly PlaylistDetailsViewController playlistDetailsViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        private Playlist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private List<IPlaylistSong> _missingSongs;

        private CancellationTokenSource tokenSource;
        private SemaphoreSlim downloadPauseSemaphore;
        private bool preferCustomArchiveURL;

        private int downloadingBeatmapCollectionIdx;
        private IAnnotatedBeatmapLevelCollection[] downloadingBeatmapLevelCollections;

        public event Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

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
            downloadPauseSemaphore = new SemaphoreSlim(0, 1);
            preferCustomArchiveURL = true;
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
            int numberOfSongs = ((IAnnotatedBeatmapLevelCollection)selectedPlaylist).beatmapLevelCollection.beatmapLevels.Length;
            string checkboxText = numberOfSongs > 0 ? $"Also delete all {numberOfSongs} songs from the game." : "";
            popupModalsController.ShowYesNoModal(rootTransform, $"Are you sure you would like to delete the playlist \"{selectedPlaylist.Title}\"?", DeleteButtonPressed, checkboxText: checkboxText);
        }

        private void DeleteButtonPressed()
        {
            try
            {
                if (popupModalsController.CheckboxValue)
                {
                    DeleteSongs();
                }
                DeletePlaylist();
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(rootTransform, "Error: Playlist cannot be deleted.", null);
                Plugin.Log.Critical(string.Format("An exception was thrown while deleting a playlist.\nException message:{0}", e));
            }
        }

        private void DeleteSongs()
        {
            IPreviewBeatmapLevel[] beatmapLevels = ((IAnnotatedBeatmapLevelCollection)selectedPlaylist).beatmapLevelCollection.beatmapLevels;
            foreach (CustomPreviewBeatmapLevel beatmapLevel in beatmapLevels.OfType<CustomPreviewBeatmapLevel>())
            {
                SongCore.Loader.Instance.DeleteSong(beatmapLevel.customLevelPath);
            }
        }

        private void DeletePlaylist()
        {
            parentManager.DeletePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
            int selectedIndex = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            List<IAnnotatedBeatmapLevelCollection> annotatedBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToList();
            annotatedBeatmapLevelCollections.RemoveAt(selectedIndex);
            selectedIndex--;
            LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections.ToArray(), selectedIndex < 0 ? 0 : selectedIndex);
        }

        #endregion

        #region Download

        [UIAction("download-click")]
        private async Task DownloadPlaylistAsync()
        {
            popupModalsController.ShowOkModal(rootTransform, "", CancelButtonPressed, "Cancel");

            UpdateMissingSongs();
            if (MissingSongs == null)
            {
                popupModalsController.OkText = "Error: The selected playlist cannot be downloaded.";
                popupModalsController.OkButtonText = "Ok";
                return;
            }

            popupModalsController.OkText = string.Format("{0}/{1} songs downloaded", 0, MissingSongs.Count);
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            preferCustomArchiveURL = true;
            bool shownCustomArchiveWarning = false;

            for (int i = 0; i < MissingSongs.Count; i++)
            {
                if (preferCustomArchiveURL && MissingSongs[i].TryGetCustomData("customArchiveURL", out object outCustomArchiveURL))
                {
                    string customArchiveURL = (string)outCustomArchiveURL;
                    string identifier = PlaylistLibUtils.GetIdentifierForPlaylistSong(MissingSongs[i]);
                    if (identifier == "")
                    {
                        continue;
                    }

                    if (!shownCustomArchiveWarning)
                    {
                        shownCustomArchiveWarning = true;
                        popupModalsController.ShowYesNoModal(rootTransform, "This playlist uses mirror download links. Would you like to use them?", 
                            CustomArchivePreferred, noButtonPressedCallback: CustomArchiveNotPreferred, animateParentCanvas: false);
                        await downloadPauseSemaphore.WaitAsync();
                        if (!preferCustomArchiveURL)
                        {
                            i--;
                            continue;
                        }
                    }
                    await DownloaderUtils.instance.BeatmapDownloadByCustomURL(customArchiveURL, identifier, tokenSource.Token);
                }
                else if (!string.IsNullOrEmpty(MissingSongs[i].Hash))
                {
                    await DownloaderUtils.instance.BeatmapDownloadByHash(MissingSongs[i].Hash, tokenSource.Token);
                }
                else if (!string.IsNullOrEmpty(MissingSongs[i].Key))
                {
                    string hash = await DownloaderUtils.instance.BeatmapDownloadByKey(MissingSongs[i].Key.ToLower(), tokenSource.Token);
                    if (!string.IsNullOrEmpty(hash))
                    {
                        MissingSongs[i].Hash = hash;
                    }
                }
                popupModalsController.OkText = string.Format("{0}/{1} songs downloaded", i + 1, MissingSongs.Count);
            }

            popupModalsController.OkText = "Download Complete!";
            popupModalsController.OkButtonText = "Ok";

            parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);

            downloadingBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToArray();
            downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            SongCore.Loader.Instance.RefreshSongs(false);
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        private void CancelButtonPressed()
        {
            tokenSource.Cancel();
        }

        private void CustomArchivePreferred()
        {
            preferCustomArchiveURL = true;
            downloadPauseSemaphore.Release();
        }

        private void CustomArchiveNotPreferred()
        {
            preferCustomArchiveURL = false;
            downloadPauseSemaphore.Release();
        }

        private void LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent()
        {
            LevelCollectionTableViewUpdatedEvent?.Invoke(downloadingBeatmapLevelCollections, downloadingBeatmapCollectionIdx);
            UpdateMissingSongs();
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        private void UpdateMissingSongs()
        {
            if (selectedPlaylist is LegacyPlaylist legacyPlaylist)
            {
                MissingSongs = legacyPlaylist.Where(s => s.PreviewBeatmapLevel == null).Distinct(IPlaylistSongComparer<IPlaylistSong>.Default).ToList();
            }
            else if (selectedPlaylist is BlistPlaylist blistPlaylist)
            {
                MissingSongs = blistPlaylist.Where(s => s.PreviewBeatmapLevel == null).Distinct(IPlaylistSongComparer<IPlaylistSong>.Default).ToList();
            }
            else
            {
                MissingSongs = null;
            }
        }

        private List<IPlaylistSong> MissingSongs
        {
            get => _missingSongs;
            set
            {
                _missingSongs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadInteractable)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadHint)));
            }
        }

        [UIValue("download-hint")]
        private string DownloadHint
        {
            get
            {
                if (DownloadInteractable)
                {
                    return $"Download {MissingSongs.Count} missing songs.";
                }
                return "All songs already downloaded";
            }
        }

        [UIValue("download-interactable")]
        private bool DownloadInteractable => MissingSongs != null && MissingSongs.Count > 0;

        #endregion

        #region Sync

        [UIAction("sync-click")]
        private async Task SyncPlaylistAsync()
        {
            if (!selectedPlaylist.TryGetCustomData("syncURL", out object outSyncURL))
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
                UpdateMissingSongs();

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
                MissingSongs = null;
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
