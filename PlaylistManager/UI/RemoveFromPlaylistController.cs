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

namespace PlaylistManager.UI
{
    class RemoveFromPlaylistController : NotifiableSingleton<RemoveFromPlaylistController>
    {
        private StandardLevelDetailViewController standardLevel;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;

        //Currently selected song data
        public IPreviewBeatmapLevel level;

        [UIComponent("remove-button")]
        private Transform removeButtonTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        internal void Setup()
        {
            standardLevel = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            annotatedBeatmapLevelCollectionsViewController = Resources.FindObjectsOfTypeAll<AnnotatedBeatmapLevelCollectionsViewController>().First();
            levelCollectionViewController = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().First();
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.RemoveFromPlaylist.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
            removeButtonTransform.localScale *= 0.7f;
        }

        internal void LevelSelected(IPreviewBeatmapLevel level)
        {
            this.level = level;
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection is CustomPlaylistSO)
            {
                removeButtonTransform.gameObject.SetActive(true);
            }
            else
            {
                removeButtonTransform.gameObject.SetActive(false);
            }
        }

        [UIAction("button-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to remove \n{0}\n from the playlist?", level.songName);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {
            Playlist selectedPlaylist = Playlist.loadedPlaylists[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            List<IPreviewBeatmapLevel> newBeatmapList = selectedPlaylist.beatmapLevelCollection.beatmapLevels.ToList();
            newBeatmapList.Remove(level);
            selectedPlaylist.editBeatMapLevels(newBeatmapList.ToArray());
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }
    }
}
