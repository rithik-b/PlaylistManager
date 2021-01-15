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
    class RemoveFromPlaylistController : ILevelCollectionUpdater
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;

        [UIComponent("remove-button")]
        private Transform removeButtonTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        private bool buttonActive;

        RemoveFromPlaylistController(StandardLevelDetailViewController standardLevel, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.standardLevelDetailViewController = standardLevel;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.RemoveFromPlaylist.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
            removeButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to remove \n{0}\n from the playlist?", standardLevelDetailViewController.selectedDifficultyBeatmap.level.songName);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {   
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = PlaylistLibUtils.LibDefaultManager.GetAllPlaylists()[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            selectedPlaylist.Remove((IPlaylistSong)standardLevelDetailViewController.selectedDifficultyBeatmap.level);
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection) =>
            removeButtonTransform.gameObject.SetActive(annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && beatmapLevelCollection is Playlist);
    }
}
