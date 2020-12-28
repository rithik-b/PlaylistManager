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
    class PlaylistViewController : NotifiableSingleton<PlaylistViewController>
    {
        private StandardLevelDetailViewController standardLevel;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionTableView levelCollectionTableView;
        private LevelCollectionViewController levelCollectionViewController;

        //Currently selected song data
        public IPreviewBeatmapLevel level;

        [UIComponent("remove-button")]
        private Transform removeButtonTransform;

        [UIComponent("modal")]
        private ModalView modal;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        internal void Setup()
        {
            standardLevel = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            annotatedBeatmapLevelCollectionsViewController = Resources.FindObjectsOfTypeAll<AnnotatedBeatmapLevelCollectionsViewController>().First();
            levelCollectionTableView = Resources.FindObjectsOfTypeAll<LevelCollectionTableView>().First();
            levelCollectionViewController = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().First();
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.PlaylistView.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
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
            levelCollectionTableView.ClearSelection();
            Playlist selectedPlaylist = LoadPlaylistScript.loadedPlaylists[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            List<IPreviewBeatmapLevel> newBeatmapList = selectedPlaylist.beatmapLevelCollection.beatmapLevels.ToList();
            newBeatmapList.Remove(level);
            LoadPlaylistScript.loadedPlaylists[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2].editBeatMapLevels(newBeatmapList.ToArray());
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionTableView.RefreshLevelsAvailability();
        }
    }
}
