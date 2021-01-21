using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using System.Reflection;
using UnityEngine;
using HMUI;
using PlaylistManager.Interfaces;
using PlaylistManager.HarmonyPatches;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;
using Zenject;

namespace PlaylistManager.UI
{
    class AddPlaylistController: IPreviewBeatmapLevelUpdater, IInitializable
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("add-button")]
        private Transform addButtonTransform;

        [UIComponent("modal")]
        private ModalView modal;

        private BeatSaberPlaylistsLib.Types.IPlaylist[] loadedplaylists;

        AddPlaylistController(StandardLevelDetailViewController standardLevel, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.standardLevelDetailViewController = standardLevel;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
        }

        [UIAction("button-click")]
        internal void ShowPlaylists()
        {
            customListTableData.data.Clear();
            loadedplaylists = PlaylistLibUtils.playlistManager.GetAllPlaylists(true);

            foreach (BeatSaberPlaylistsLib.Types.IPlaylist playlist in loadedplaylists)
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
            loadedplaylists[index].Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
            customListTableData.tableView.ClearSelection();
            if(annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && AnnotatedBeatmapLevelCollectionsViewController_SetData.isCustomBeatmapLevelPack)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(AnnotatedBeatmapLevelCollectionsViewController_SetData.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            }
            PlaylistLibUtils.playlistManager.StorePlaylist(loadedplaylists[index]);
            modal.Hide(true);
        }

        [UIAction("keyboard-enter")]
        internal void CreatePlaylist(string playlistName)
        {
            PlaylistLibUtils.CreatePlaylist(playlistName, "PlaylistManager");
            ShowPlaylists();
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if (beatmapLevel.levelID.EndsWith(" WIP") || beatmapLevel is IPlaylistSong)
            {
                addButtonTransform.gameObject.SetActive(false);
            }
            else
            {
                addButtonTransform.gameObject.SetActive(true);
            }
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.AddPlaylist.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
            addButtonTransform.localScale *= 0.7f;
        }
    }
}
