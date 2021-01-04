using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using PlaylistLoaderLite;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;
using System;
using PlaylistManager.Interfaces;

namespace PlaylistManager.UI
{
    class RemoveFromPlaylistController : ILevelCollectionUpdater
    {
        private StandardLevelDetailViewController standardLevel;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;

        [UIComponent("remove-button")]
        private Transform removeButtonTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        RemoveFromPlaylistController(StandardLevelDetailViewController standardLevel, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.standardLevel = standardLevel;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.RemoveFromPlaylist.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
            removeButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to remove \n{0}\n from the playlist?", standardLevel.selectedDifficultyBeatmap.level.songName);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {
            Playlist selectedPlaylist = Playlist.loadedPlaylists[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            List<IPreviewBeatmapLevel> newBeatmapList = selectedPlaylist.beatmapLevelCollection.beatmapLevels.ToList();
            newBeatmapList.Remove(standardLevel.selectedDifficultyBeatmap.level);
            selectedPlaylist.editBeatMapLevels(newBeatmapList.ToArray());
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection)
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && beatmapLevelCollection is CustomPlaylistSO)
            {
                removeButtonTransform.gameObject.SetActive(true);
            }
            else
            {
                removeButtonTransform.gameObject.SetActive(false);
            }
        }
    }
}
