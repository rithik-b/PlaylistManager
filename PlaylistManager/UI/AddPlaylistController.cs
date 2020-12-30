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

namespace PlaylistManager.UI
{
    class AddPlaylistController : NotifiableSingleton<AddPlaylistController>
    {
        private StandardLevelDetailViewController standardLevel;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        //Currently selected song data
        public IPreviewBeatmapLevel level;

        [UIComponent("add-button")]
        private Transform addButtonTransform;

        [UIComponent("modal")]
        private ModalView modal;

        private List<Playlist> loadedplaylists;
        internal void Setup()
        {
            standardLevel = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            annotatedBeatmapLevelCollectionsViewController = Resources.FindObjectsOfTypeAll<AnnotatedBeatmapLevelCollectionsViewController>().First();
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.AddPlaylist.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
            addButtonTransform.localScale *= 0.7f;
        }

        internal void LevelSelected(IPreviewBeatmapLevel level)
        {
            this.level = level;
            if (level.levelID.EndsWith(" WIP") || (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection is CustomPlaylistSO))
            {
                addButtonTransform.gameObject.SetActive(false);
            }
            else
            {
                addButtonTransform.gameObject.SetActive(true);
            }
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
            loadedplaylists[index].editBeatMapLevels(loadedplaylists[index].beatmapLevelCollection.beatmapLevels.Append<IPreviewBeatmapLevel>(level).ToArray());
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
    }
}
