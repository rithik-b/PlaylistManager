using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.Reflection;
using TMPro;
using UnityEngine;
using PlaylistManager.Interfaces;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;
using System.Collections.Generic;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Blist;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;
using Zenject;
using PlaylistManager.HarmonyPatches;

namespace PlaylistManager.UI
{
    class PlaylistViewController : ILevelCollectionUpdater, IInitializable, IDisposable
    {
        private LevelPackDetailViewController levelPackDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;

        [UIComponent("bg")]
        private Transform bgTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        [UIComponent("ok-message")]
        private TextMeshProUGUI okMessage;

        [UIComponent("ok-modal")]
        private ModalView okModal;

        [UIComponent("download-message")]
        private TextMeshProUGUI downloadMessage;

        [UIComponent("download-modal")]
        private ModalView downloadModal;

        private CancellationTokenSource tokenSource;
        private int downloadingBeatmapCollectionIdx;

        PlaylistViewController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            this.tokenSource = new CancellationTokenSource();
        }

        [UIAction("delete-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to delete \n{0}?", annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection.collectionName);
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
                okMessage.text = "Error: Playlist cannot be deleted.";
                okModal.Show(true);
            }
        }

        [UIAction("download-click")]
        internal async System.Threading.Tasks.Task DownloadPlaylistAsync()
        {
            IAnnotatedBeatmapLevelCollection selectedPlaylist = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            List<IPlaylistSong> missingSongs;
            DownloaderUtils.Init();
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
                okMessage.text = "Error: The selected playlist cannot be downloaded.";
                okModal.Show(true);
                return;
            }
            downloadMessage.text = string.Format("{0}/{1} songs downloaded", 0, missingSongs.Count);
            downloadModal.Show(true);
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            for(int i = 0; i < missingSongs.Count; i++)
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
                    downloadMessage.text = string.Format("{0}/{1} songs downloaded", i + 1, missingSongs.Count);
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
            downloadModal.Hide(true);
            SongCore.Loader.Instance.RefreshSongs(false);
            downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
            LevelFilteringNavigationController_UpdateCustomSongs.CustomSongsUpdatedEvent += LevelFilteringNavigationController_UpdateCustomSongs_CustomSongsUpdatedEvent;
        }

        [UIAction("cancel-click")]
        internal void CancelDownload()
        {
            okMessage.text = "The download is cancelled";
            okModal.Show(true);
            tokenSource.Cancel();
        }

        private void LevelFilteringNavigationController_UpdateCustomSongs_CustomSongsUpdatedEvent()
        {
            SelectAnnotatedBeatmapCollectionByIdx(downloadingBeatmapCollectionIdx);
        }

        private void SelectAnnotatedBeatmapCollectionByIdx(int index)
        {
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.AnnotatedBeatmapLevelCollectionsViewController_SetData.otherCustomBeatmapLevelCollections, index, false);
            IAnnotatedBeatmapLevelCollection selectedCollection = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            levelCollectionViewController.SetData(selectedCollection.beatmapLevelCollection, selectedCollection.collectionName, selectedCollection.coverImage, false, null);
            levelPackDetailViewController.SetData((IBeatmapLevelPack)selectedCollection);
            LevelCollectionUpdated(selectedCollection);
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

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.PlaylistView.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            bgTransform.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            LevelFilteringNavigationController_UpdateCustomSongs.CustomSongsUpdatedEvent -= LevelFilteringNavigationController_UpdateCustomSongs_CustomSongsUpdatedEvent;
        }
    }
}
