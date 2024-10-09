﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Configuration;
using PlaylistManager.Interfaces;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPA.Loader;
using PlaylistManager.Downloaders;
using SiraUtil.Zenject;
using SongCore;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistDetailViewButtonsController : IInitializable, IDisposable, INotifyPropertyChanged, ILevelCollectionUpdater, ILevelCategoryUpdater, ILevelCollectionsTableUpdater
    {
        private readonly IHttpService siraHttpService;
        private readonly PlaylistSequentialDownloader playlistDownloader;
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private readonly PlaylistDetailsViewController playlistDetailsViewController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly Loader loader;
        private readonly PluginMetadata pluginMetadata;
        private readonly BSMLParser bsmlParser;

        private IPlaylist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private List<IPlaylistSong> _missingSongs;
        private DownloadQueueEntry _downloadQueueEntry;

        public event Action<IReadOnlyList<BeatmapLevelPack>, int> LevelCollectionTableViewUpdatedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("root")]
        private readonly Transform rootTransform;

        [UIComponent("sync-button")]
        private readonly Transform syncButtonTransform;

        internal PlaylistDetailViewButtonsController(IHttpService siraHttpService, PlaylistSequentialDownloader playlistDownloader, LevelPackDetailViewController levelPackDetailViewController,
            PopupModalsController popupModalsController, PlaylistDetailsViewController playlistDetailsViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, Loader loader, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.siraHttpService = siraHttpService;
            this.playlistDownloader = playlistDownloader;
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;
            this.playlistDetailsViewController = playlistDetailsViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.loader = loader;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;
        }

        public void Initialize()
        {
            bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.PlaylistDetailViewButtons.bsml"), levelPackDetailViewController._detailWrapper.gameObject, this);
            syncButtonTransform.transform.localScale *= 0.6f;
            syncButtonTransform.gameObject.SetActive(false);
            rootTransform.gameObject.SetActive(false);

            levelPackDetailViewController.didActivateEvent += PackViewActivated;
            playlistDownloader.QueueUpdatedEvent += OnQueueUpdated;
        }

        public void Dispose()
        {
            levelPackDetailViewController.didActivateEvent -= PackViewActivated;
            playlistDownloader.QueueUpdatedEvent -= OnQueueUpdated;
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
            var numberOfSongs = selectedPlaylist.PlaylistLevelPack.AllBeatmapLevels().Count;
            var checkboxText = numberOfSongs > 0 ? $"Also delete all {numberOfSongs} songs from the game." : "";
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

        private async void DeleteSongs()
        {
            popupModalsController.ShowLoadingModal(rootTransform, "Deleting Playlist & Songs");

            var levelPaths = selectedPlaylist.BeatmapLevels
                .Where(l => !l.hasPrecalculatedData)
                .Select(l => SongCore.Collections.GetLoadedSaveData(l.levelID)?.customLevelFolderInfo.folderPath)
                .Where(p => p != null)
                .ToList();
            await loader.DeleteSongsAsync(levelPaths);

            popupModalsController.DismissLoadingModal();
        }

        private void DeletePlaylist()
        {
            parentManager.DeletePlaylist(selectedPlaylist, true);
            var selectedIndex = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            var annotatedBeatmapLevelCollections = annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollections.ToList();
            annotatedBeatmapLevelCollections.RemoveAt(selectedIndex);
            selectedIndex--;
            LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections.ToArray(), selectedIndex < 0 ? 0 : selectedIndex);
        }

        #endregion

        #region Download

        [UIAction("download-click")]
        private void DownloadClick()
        {
            DownloadQueueEntry = new DownloadQueueEntry(selectedPlaylist, parentManager);
            playlistDownloader.QueuePlaylist(DownloadQueueEntry);
            popupModalsController.ShowOkModal(rootTransform, $"{selectedPlaylist.Title} has been added to the download queue!", null);
        }

        private void OnQueueUpdated()
        {
            if (PlaylistSequentialDownloader.downloadQueue.Count == 0)
            {
                DownloadQueueEntry = null;
                UpdateMissingSongs();
            }
        }

        private void UpdateMissingSongs() => MissingSongs = PlaylistLibUtils.GetMissingSongs(selectedPlaylist);

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

        private DownloadQueueEntry DownloadQueueEntry
        {
            get => _downloadQueueEntry;
            set
            {
                _downloadQueueEntry = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadInteractable)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadHint)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeleteHint)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistNotDownloading)));
            }
        }

        [UIValue("download-hint")]
        private string DownloadHint
        {
            get
            {
                if (DownloadQueueEntry != null)
                {
                    return "Playlist is downloading";
                }

                if (MissingSongs != null && MissingSongs.Count > 0)
                {
                    return $"Download {MissingSongs.Count} missing songs.";
                }

                return "All songs already downloaded";
            }
        }

        [UIValue("delete-hint")]
        private string DeleteHint
        {
            get
            {
                if (DownloadQueueEntry != null)
                {
                    return "Can't delete playlist when it is downloading";
                }

                return "Delete Playlist";
            }
        }

        [UIValue("download-interactable")]
        private bool DownloadInteractable => DownloadQueueEntry == null && MissingSongs != null && MissingSongs.Count > 0;


        [UIValue("playlist-not-downloading")]
        private bool PlaylistNotDownloading => DownloadQueueEntry == null;

        #endregion

        #region Sync

        [UIAction("sync-click")]
        private async Task SyncPlaylistAsync()
        {
            if (!selectedPlaylist.TryGetCustomData("syncURL", out var outSyncURL))
            {
                popupModalsController.ShowOkModal(rootTransform, "Error: The selected playlist cannot be synced", null);
                return;
            }

            var syncURL = (string)outSyncURL;
            var tokenSource = new CancellationTokenSource();

            popupModalsController.ShowOkModal(rootTransform, "Syncing Playlist", () => tokenSource.Cancel(), "Cancel");
            Stream playlistStream = null;

            try
            {
                var httpResponse = await siraHttpService.GetAsync(syncURL, cancellationToken: tokenSource.Token);
                if (httpResponse.Successful)
                {
                    selectedPlaylist.Clear(); // Clear all songs
                    PlaylistLibUtils.playlistManager.DefaultHandler.Populate(await httpResponse.ReadAsStreamAsync(), selectedPlaylist);
                    selectedPlaylist.RaisePlaylistChanged();
                    parentManager.StorePlaylist(selectedPlaylist);
                }
                else
                {
                    popupModalsController.OkText = "Error: The selected playlist cannot be synced";
                    popupModalsController.OkButtonText = "Ok";
                    return;
                }
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
                switch (PluginConfig.Instance.SyncOption)
                {
                    case PluginConfig.SyncOptions.On:
                        DownloadAccepted();
                        break;
                    case PluginConfig.SyncOptions.Off:
                        DownloadRejected();
                        break;
                    case PluginConfig.SyncOptions.Ask:
                        popupModalsController.ShowYesNoModal(rootTransform, "Would you like to download the songs after syncing?", DownloadAccepted, noButtonPressedCallback: DownloadRejected);
                        break;
                }
            }
        }

        private void DownloadAccepted()
        {
            DownloadQueueEntry = new DownloadQueueEntry(selectedPlaylist, parentManager);
            playlistDownloader.QueuePlaylist(DownloadQueueEntry);
            popupModalsController.ShowOkModal(rootTransform, "Playlist Synced and added to Download Queue!", null);
        }

        private void DownloadRejected() => popupModalsController.ShowOkModal(rootTransform, "Playlist Synced!", null);

        #endregion

        private void PackViewActivated(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (selectedPlaylist != null && parentManager != null)
            {
                rootTransform.gameObject.SetActive(true);
            }
        }

        public void LevelCollectionUpdated(BeatmapLevelPack selectedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (selectedBeatmapLevelCollection is PlaylistLevelPack playlistLevelPack)
            {
                selectedPlaylist = playlistLevelPack.playlist;
                this.parentManager = parentManager;
                DownloadQueueEntry = PlaylistSequentialDownloader.downloadQueue.OfType<DownloadQueueEntry>().FirstOrDefault(x => x.playlist == selectedPlaylist);
                UpdateMissingSongs();

                rootTransform.gameObject.SetActive(true);
                if (playlistLevelPack.playlist.TryGetCustomData("syncURL", out var syncURLObj) && syncURLObj is string syncURL && !string.IsNullOrWhiteSpace(syncURL))
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
                selectedPlaylist = null;
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
