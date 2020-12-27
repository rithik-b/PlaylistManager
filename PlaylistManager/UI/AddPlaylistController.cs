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

namespace PlaylistManager.UI
{
    class AddPlaylistController : NotifiableSingleton<AddPlaylistController>
    {
        private StandardLevelDetailViewController standardLevel;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        //Currently selected song data
        public IPreviewBeatmapLevel level;

        [UIComponent("add-button")]
        private Transform addButtonTransform;

        [UIComponent("modal")]
        private ModalView modal;

        private Playlist[] loadedplaylists;
        internal void Setup()
        {
            standardLevel = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.AddPlaylist.bsml"), standardLevel.transform.Find("LevelDetail").gameObject, this);
            addButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void ShowPlaylists()
        {
            level = standardLevel.selectedDifficultyBeatmap.level;
            customListTableData.data.Clear();
            loadedplaylists = LoadPlaylistScript.loadedPlaylists;

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
            modal.Hide(true);
        }
    }
}
