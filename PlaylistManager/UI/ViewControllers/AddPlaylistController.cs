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
using PlaylistManager.Configuration;
using BeatSaberPlaylistsLib.Types;

namespace PlaylistManager.UI
{
    class AddPlaylistController: IPlaylistManagerModal
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;

        private BeatSaberPlaylistsLib.Types.IPlaylist[] loadedplaylists;
        public bool parsed { get; private set; }

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("modal")]
        private readonly ModalView modal;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIComponent("keyboard")]
        private readonly RectTransform keyboardTransform;

        public AddPlaylistController(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            parsed = false;
        }

        internal void Parse()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.AddPlaylist.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
            modalPosition = modalTransform.position; // Position can change if SongBrowser is clicked while modal is opened so storing here
            parsed = true;
        }

        internal void ShowPlaylists()
        {
            modal.Show(true);
            customListTableData.data.Clear();
            loadedplaylists = PlaylistLibUtils.playlistManager.GetAllPlaylists(true);

            foreach (BeatSaberPlaylistsLib.Types.IPlaylist playlist in loadedplaylists)
            {
                if (playlist is IDeferredSpriteLoad deferredSpriteLoadPlaylist && !deferredSpriteLoadPlaylist.SpriteWasLoaded)
                {
                    _ = playlist.coverImage;
                    deferredSpriteLoadPlaylist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                    deferredSpriteLoadPlaylist.SpriteLoaded += DeferredSpriteLoadPlaylist_SpriteLoaded;
                }
                else
                {
                    ShowPlaylist(playlist);
                }
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, false);
        }

        private void DeferredSpriteLoadPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is IDeferredSpriteLoad deferredSpriteLoadPlaylist)
            {
                ShowPlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)deferredSpriteLoadPlaylist);
                customListTableData.tableView.ReloadData();
                (deferredSpriteLoadPlaylist).SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
            }
        }

        private void ShowPlaylist(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            string subName = string.Format("{0} songs", playlist.beatmapLevelCollection.beatmapLevels.Length);
            if (Array.Exists(playlist.beatmapLevelCollection.beatmapLevels, level => level.levelID == standardLevelDetailViewController.selectedDifficultyBeatmap.level.levelID))
            {
                subName += " (contains song)";
            }
            customListTableData.data.Add(new CustomCellInfo(playlist.collectionName, subName, playlist.coverImage));
        }

        [UIAction("select-cell")]
        internal void OnCellSelect(TableView tableView, int index)
        {
            loadedplaylists[index].Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
            customListTableData.tableView.ClearSelection();
            PlaylistLibUtils.playlistManager.StorePlaylist(loadedplaylists[index]);
            modal.Hide(true);
        }

        [UIAction("keyboard-opened")]
        internal void OnKeyboardOpened()
        {
            // Need to set parent because it goes out of order
            if (parsed && modalTransform != null && keyboardTransform != null)
            {
                keyboardTransform.transform.SetParent(modalTransform);
            }
        }

        [UIAction("keyboard-enter")]
        internal void CreatePlaylist(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return;
            }
            if (!PluginConfig.Instance.DefaultImageDisabled)
            {
                PlaylistLibUtils.CreatePlaylist(playlistName, PluginConfig.Instance.AuthorName);
            }
            else
            {
                PlaylistLibUtils.CreatePlaylist(playlistName, PluginConfig.Instance.AuthorName, "");
            }
            ShowPlaylists();
        }

        public void ParentControllerDeactivated()
        {
            // Need to restore position and parent of modal
            if (parsed && rootTransform != null && modalTransform != null)
            {
                modalTransform.transform.SetParent(rootTransform);
                modalTransform.position = modalPosition;
            }
        }
    }
}
