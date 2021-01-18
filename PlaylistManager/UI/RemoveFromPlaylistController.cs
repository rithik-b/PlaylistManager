using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using TMPro;
using UnityEngine;
using PlaylistManager.Interfaces;
using BeatSaberPlaylistsLib.Types;
using System;
using PlaylistManager.Utilities;

namespace PlaylistManager.UI
{
    class RemoveFromPlaylistController : ILevelCollectionUpdater, IPreviewBeatmapLevelUpdater
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;
        private IPlaylistSong selectedPlaylistSong;

        [UIComponent("remove-button")]
        private Transform removeButtonTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        private bool buttonActive;

        RemoveFromPlaylistController(StandardLevelDetailViewController standardLevelDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.RemoveFromPlaylist.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
            removeButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to remove \n{0}\n from the playlist?", selectedPlaylistSong.songName);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {   
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = PlaylistLibUtils.playlistManager.GetAllPlaylists()[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            selectedPlaylist.Remove((IPlaylistSong)selectedPlaylistSong);
            PlaylistLibUtils.playlistManager.StorePlaylist(selectedPlaylist);
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection) =>
            removeButtonTransform.gameObject.SetActive(annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && beatmapLevelCollection is Playlist);

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if(beatmapLevel is IPlaylistSong)
            {
                selectedPlaylistSong = (IPlaylistSong)beatmapLevel;
            }
        }
    }
}
