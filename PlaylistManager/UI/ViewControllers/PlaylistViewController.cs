using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.Reflection;
using TMPro;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;
using System.Collections.Generic;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Blist;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;
using PlaylistManager.HarmonyPatches;
using UnityEngine;
using PlaylistManager.Interfaces;
using System.IO;

namespace PlaylistManager.UI
{
    public class PlaylistViewController : IDisposable, IPlaylistManagerModal, IRefreshable
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelCollectionViewController levelCollectionViewController;

        [UIComponent("modal")]
        private readonly ModalView modal;

        [UIComponent("modal-message")]
        private readonly TextMeshProUGUI modalMessage;

        [UIComponent("modal-button")]
        private readonly TextMeshProUGUI modalButtonText;

        [UIComponent("delete-modal")]
        private readonly ModalView deleteModal;

        [UIComponent("warning-message")]
        private readonly TextMeshProUGUI warningMessage;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIComponent("delete-modal")]
        private readonly RectTransform deleteModalTransform;

        private Vector3 deleteModalPosition;

        internal enum ModalState
        {
            OkModal,
            DownloadingModal
        }

        private ModalState _modalState;

        internal ModalState CurrentModalState
        {
            get
            {
                return _modalState;
            }
            set
            {
                _modalState = value;
                switch (_modalState)
                {
                    case ModalState.DownloadingModal:
                        modalButtonText.text = "Cancel";
                        break;
                    default:
                        modalButtonText.text = "Ok";
                        break;
                }
            }
        }

        private CancellationTokenSource tokenSource;
        private int downloadingBeatmapCollectionIdx;
        internal bool parsed;
        internal event Action<IAnnotatedBeatmapLevelCollection> didSelectAnnotatedBeatmapLevelCollectionEvent;

        public PlaylistViewController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            tokenSource = new CancellationTokenSource();
            parsed = false;
        }

        internal void Parse()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistView.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            modalPosition = modalTransform.position;
            deleteModalPosition = deleteModalTransform.position;
        }

        public void Dispose()
        {
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        [UIAction("delete-click")]
        internal void DisplayWarning()
        {
            deleteModal.Show(true);
            warningMessage.text = string.Format("Are you sure you would like to delete {0}?", annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection.collectionName);
        }

        [UIAction("delete-confirm")]
        internal void DeletePlaylist()
        {
            if (PlaylistLibUtils.playlistManager.DeletePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection))
            {
                SelectAnnotatedBeatmapCollectionByIdx(annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 1);
            }
            else
            {
                modalMessage.text = "Error: Playlist cannot be deleted.";
                CurrentModalState = ModalState.OkModal;
                modal.Show(true);
            }
        }

        internal async Task DownloadPlaylistAsync()
        {
            var selectedBeatmapLevelCollection = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            List<IPlaylistSong> missingSongs;
            if (selectedBeatmapLevelCollection is BlistPlaylist blistPlaylist)
            {
                missingSongs = blistPlaylist.Where(s => s.PreviewBeatmapLevel == null).Select(s => s).ToList();
            }
            else if (selectedBeatmapLevelCollection is LegacyPlaylist legacyPlaylist)
            {
                missingSongs = legacyPlaylist.Where(s => s.PreviewBeatmapLevel == null).Select(s => s).ToList();
            }
            else
            {
                modalMessage.text = "Error: The selected playlist cannot be downloaded.";
                CurrentModalState = ModalState.OkModal;
                modal.Show(true);
                return;
            }

            modalMessage.text = string.Format("{0}/{1} songs downloaded", 0, missingSongs.Count);
            CurrentModalState = ModalState.DownloadingModal;
            modal.Show(true);
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            for (int i = 0; i < missingSongs.Count; i++)
            {
                try
                {
                    if (!string.IsNullOrEmpty(missingSongs[i].Key))
                    {
                        await DownloaderUtils.instance.BeatmapDownloadByKey(missingSongs[i].Key.ToLower(), tokenSource.Token);
                    }
                    else if (!string.IsNullOrEmpty(missingSongs[i].Hash))
                    {
                        await DownloaderUtils.instance.BeatmapDownloadByHash(missingSongs[i].Hash, tokenSource.Token);
                    }
                    modalMessage.text = string.Format("{0}/{1} songs downloaded", i + 1, missingSongs.Count);
                }
                catch (Exception e)
                {
                    if (!(e is TaskCanceledException))
                    {
                        Plugin.Log.Critical("Failed to download Song!");
                    }
                    break;
                }
            }
            modal.Hide(true);
            SongCore.Loader.Instance.RefreshSongs(false);
            downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        internal async Task SyncPlaylistAsync()
        {
            var selectedBeatmapLevelCollection = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            if (!(selectedBeatmapLevelCollection is Playlist))
            {
                modalMessage.text = "Error: The selected playlist cannot be synced";
                CurrentModalState = ModalState.OkModal;
                modal.Show(true);
                return;
            }
            var selectedPlaylist = (Playlist)selectedBeatmapLevelCollection;
            if (selectedPlaylist.CustomData == null || !selectedPlaylist.CustomData.ContainsKey("syncURL"))
            {
                modalMessage.text = "Error: The selected playlist cannot be synced";
                CurrentModalState = ModalState.OkModal;
                modal.Show(true);
                return;
            }

            string path = Path.Combine(PlaylistLibUtils.playlistManager.PlaylistPath, selectedPlaylist.Filename + '.' + selectedPlaylist.SuggestedExtension);
            string syncURL = (string)selectedPlaylist.CustomData["syncURL"];
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            modalMessage.text = "Syncing Playlist";
            CurrentModalState = ModalState.DownloadingModal;
            modal.Show(true);

            Stream playlistStream = null;
            try
            {
                playlistStream = new MemoryStream(await DownloaderUtils.instance.DownloadFileToBytesAsync(syncURL, tokenSource.Token));
                modal.Show(false);
                ((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist).RemoveAll((playlistSong) => true); // Clear all songs
                PlaylistLibUtils.playlistManager.DefaultHandler.Populate(playlistStream, (BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
            }
            catch (Exception e)
            {
                modal.Show(false);
                if (!(e is TaskCanceledException))
                {
                    modalMessage.text = "Error: The selected playlist cannot be synced";
                    CurrentModalState = ModalState.OkModal;
                    modal.Show(true);
                }
                return;
            }
            finally
            {
                // If the downloaded playlist doesn't have the sync url, add it back
                if (selectedPlaylist.CustomData == null)
                {
                    selectedPlaylist.CustomData = new Dictionary<string, object>();
                }
                if (!selectedPlaylist.CustomData.ContainsKey("syncURL"))
                {
                    selectedPlaylist.CustomData["syncURL"] = syncURL;
                }

                PlaylistLibUtils.playlistManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                await DownloadPlaylistAsync();

                modalMessage.text = "Playlist Synced";
                CurrentModalState = ModalState.OkModal;
                modal.Show(true);
            }
        }

        [UIAction("click-modal-button")]
        internal void OkClicked()
        {
            if(CurrentModalState == ModalState.DownloadingModal)
            {
                tokenSource.Cancel();
            }
        }

        private void LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent()
        {
            SelectAnnotatedBeatmapCollectionByIdx(downloadingBeatmapCollectionIdx);
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        private void SelectAnnotatedBeatmapCollectionByIdx(int index)
        {
            // TODO FIX!!!!
            // annotatedBeatmapLevelCollectionsViewController.SetData(AnnotatedBeatmapLevelCollectionsViewController_SetData.otherCustomBeatmapLevelCollections, index, false);
            IAnnotatedBeatmapLevelCollection selectedCollection = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            levelCollectionViewController.SetData(selectedCollection.beatmapLevelCollection, selectedCollection.collectionName, selectedCollection.coverImage, false, null);
            levelPackDetailViewController.SetData((IBeatmapLevelPack)selectedCollection);
            didSelectAnnotatedBeatmapLevelCollectionEvent?.Invoke(selectedCollection);
        }

        public void ParentControllerDeactivated()
        {
            if (parsed && rootTransform != null && modalTransform != null && deleteModalTransform != null)
            {
                modalTransform.transform.SetParent(rootTransform);
                modalTransform.position = modalPosition;

                deleteModalTransform.transform.SetParent(rootTransform);
                deleteModalTransform.position = deleteModalPosition;
            }
        }

        public void Refresh()
        {
            SelectAnnotatedBeatmapCollectionByIdx(annotatedBeatmapLevelCollectionsViewController.selectedItemIndex);
        }
    }
}
