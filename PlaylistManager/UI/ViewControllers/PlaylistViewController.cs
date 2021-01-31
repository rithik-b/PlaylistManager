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

namespace PlaylistManager.UI
{
    class PlaylistViewController : IDisposable, IPlaylistManagerModal
    {
        private LevelPackDetailViewController levelPackDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;

        [UIComponent("delete-modal")]
        private ModalView deleteModal;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        [UIComponent("modal")]
        private ModalView modal;

        [UIComponent("modal-message")]
        private TextMeshProUGUI modalMessage;

        [UIComponent("modal-button")]
        private TextMeshProUGUI modalButtonText;

        [UIComponent("root")]
        private RectTransform rootTransform;

        [UIComponent("delete-modal")]
        private RectTransform deleteModalTransform;

        [UIComponent("modal")]
        private RectTransform modalTransform;

        internal enum ModalState
        {
            OkModal,
            DownloadingModal
        }

        private ModalState _modalState;

        internal ModalState modalState
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

        PlaylistViewController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
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
                modalState = ModalState.OkModal;
                modal.Show(true);
            }
        }

        internal async System.Threading.Tasks.Task DownloadPlaylistAsync()
        {
            IAnnotatedBeatmapLevelCollection selectedPlaylist = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            List<IPlaylistSong> missingSongs;
            if (selectedPlaylist is BlistPlaylist)
            {
                missingSongs = ((BlistPlaylist)selectedPlaylist).Where(s => s.PreviewBeatmapLevel == null).Select(s => s).ToList();
            }
            else if(selectedPlaylist is LegacyPlaylist)
            {
                missingSongs = ((LegacyPlaylist)selectedPlaylist).Where(s => s.PreviewBeatmapLevel == null).Select(s => s).ToList();
            }
            else
            {
                modalMessage.text = "Error: The selected playlist cannot be downloaded.";
                modalState = ModalState.OkModal;
                modal.Show(true);
                return;
            }

            modalMessage.text = string.Format("{0}/{1} songs downloaded", 0, missingSongs.Count);
            modalState = ModalState.DownloadingModal;
            modal.Show(true);
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            for (int i = 0; i < missingSongs.Count; i++)
            {
                try
                {
                    if (!string.IsNullOrEmpty(missingSongs[i].Key))
                    {
                        await DownloaderUtils.instance.BeatmapDownloadByKey(missingSongs[i].Key, tokenSource.Token);
                    }
                    else if (!string.IsNullOrEmpty(missingSongs[i].Hash))
                    {
                        await DownloaderUtils.instance.BeatmapDownloadByHash(missingSongs[i].Hash, tokenSource.Token);
                    }
                    modalMessage.text = string.Format("{0}/{1} songs downloaded", i + 1, missingSongs.Count);
                }
                catch (Exception e)
                {
                    if (e is TaskCanceledException)
                        Plugin.Log.Warn("Song Download Aborted.");
                    else
                        Plugin.Log.Critical("Failed to download Song!");
                    break;
                }
            }
            modal.Hide(true);
            SongCore.Loader.Instance.RefreshSongs(false);
            downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_UpdateSecondChildControllerContent_SecondChildControllerUpdatedEvent;
        }

        [UIAction("click-modal-button")]
        internal void OkClicked()
        {
            if(modalState == ModalState.DownloadingModal)
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
            annotatedBeatmapLevelCollectionsViewController.SetData(AnnotatedBeatmapLevelCollectionsViewController_SetData.otherCustomBeatmapLevelCollections, index, false);
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
                deleteModalTransform.transform.SetParent(rootTransform);
            }
        }
    }
}
