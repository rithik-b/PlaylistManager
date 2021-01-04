using System;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using System.Reflection;
using UnityEngine;
using PlaylistLoaderLite;
using HMUI;
using System.Collections.Generic;
using PlaylistManager.Interfaces;

namespace PlaylistManager.UI
{
    class AddPlaylistController: ILevelCollectionUpdater
    {
        private StandardLevelDetailViewController standardLevel;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("add-button")]
        private Transform addButtonTransform;

        [UIComponent("modal")]
        private ModalView modal;

        private List<Playlist> loadedplaylists;
        AddPlaylistController(StandardLevelDetailViewController standardLevel, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.standardLevel = standardLevel;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.AddPlaylist.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
            addButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void ShowPlaylists()
        {
            customListTableData.data.Clear();
            loadedplaylists = Playlist.loadedPlaylists;

            foreach (Playlist playlist in loadedplaylists)
            {
                String subName = String.Format("{0} songs", playlist.beatmapLevelCollection.beatmapLevels.Length);
                customListTableData.data.Add(new CustomCellInfo(playlist.collectionName, subName, playlist.coverImage));
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, false);
        }

        [UIAction("select-cell")]
        internal void OnCellSelect(TableView tableView, int index)
        {
            loadedplaylists[index].editBeatMapLevels(loadedplaylists[index].beatmapLevelCollection.beatmapLevels.Append<IPreviewBeatmapLevel>(standardLevel.selectedDifficultyBeatmap.level).ToArray());
            customListTableData.tableView.ClearSelection();
            if(annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            }
            modal.Hide(true);
        }

        [UIAction("keyboard-enter")]
        internal void CreatePlaylist(string playlistName)
        {
            Playlist.CreatePlaylist(playlistName, "PlaylistManager");
            ShowPlaylists();
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection)
        {
            IPreviewBeatmapLevel level = standardLevel.selectedDifficultyBeatmap.level;
            if (level.levelID.EndsWith(" WIP") || (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && beatmapLevelCollection is CustomPlaylistSO))
            {
                addButtonTransform.gameObject.SetActive(false);
            }
            else
            {
                addButtonTransform.gameObject.SetActive(true);
            }
        }
    }
}
