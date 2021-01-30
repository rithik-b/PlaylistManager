using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using System.Reflection;
using HMUI;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Utilities;
using PlaylistManager.Interfaces;
using UnityEngine;

namespace PlaylistManager.UI
{
    class AddPlaylistController: IPlaylistManagerModal
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("modal")]
        private ModalView modal;

        [UIComponent("root")]
        private RectTransform rootTransform;

        [UIComponent("modal")]
        private RectTransform modalTransform;

        [UIComponent("keyboard")]
        private RectTransform keyboardTransform;

        private BeatSaberPlaylistsLib.Types.IPlaylist[] loadedplaylists;
        internal bool parsed;

        AddPlaylistController(StandardLevelDetailViewController standardLevelDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            parsed = false;
        }

        internal void Parse()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.AddPlaylist.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
        }

        internal void ShowPlaylists()
        {
            modal.Show(true);
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

        public void ParentControllerDeactivated()
        {
            if(parsed && rootTransform != null && modalTransform != null && keyboardTransform != null)
            {
                modalTransform.transform.SetParent(rootTransform);
                keyboardTransform.transform.SetParent(modalTransform);
            }
        }
    }
}
