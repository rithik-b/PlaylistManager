using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using System.Reflection;
using System.Linq;
using HMUI;
using PlaylistManager.Utilities;
using UnityEngine;
using PlaylistManager.Configuration;
using BeatSaberPlaylistsLib.Types;
using BeatSaberMarkupLanguage.Parser;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

namespace PlaylistManager.UI
{
    public class AddPlaylistModalController : INotifyPropertyChanged
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly PopupModalsController popupModalsController;

        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private BeatSaberPlaylistsLib.PlaylistManager[] childManagers;
        private List<BeatSaberPlaylistsLib.Types.IPlaylist> childPlaylists;

        private readonly Sprite folderIcon;
        private bool parsed;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("highlight-checkbox")]
        private readonly RectTransform highlightCheckboxTransform;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public AddPlaylistModalController(StandardLevelDetailViewController standardLevelDetailViewController, PopupModalsController popupModalsController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.popupModalsController = popupModalsController;
            folderIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.FolderIcon.png");
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.AddPlaylistModal.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
                modalPosition = modalTransform.position; // Position can change if SongBrowser is clicked while modal is opened so storing here
                highlightCheckboxTransform.transform.localScale *= 0.5f;
                parsed = true;
            }
            modalTransform.position = modalPosition; // Reset position
        }

        #region Show Playlists

        internal void ShowModal()
        {
            Parse();
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            ShowPlaylistsForManager(PlaylistLibUtils.playlistManager);
        }

        internal void ShowPlaylistsForManager(BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            customListTableData.data.Clear();

            this.parentManager = parentManager;
            childManagers = parentManager.GetChildManagers().ToArray();
            var childPlaylists = parentManager.GetAllPlaylists(false);
            this.childPlaylists = childPlaylists.ToList();

            foreach (BeatSaberPlaylistsLib.PlaylistManager playlistManager in childManagers)
            {
                customListTableData.data.Add(new CustomCellInfo(Path.GetFileName(playlistManager.PlaylistPath), "Folder", folderIcon));
            }
            foreach (BeatSaberPlaylistsLib.Types.IPlaylist playlist in childPlaylists)
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
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
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
                if (!playlist.AllowDuplicates)
                {
                    childPlaylists.Remove(playlist);
                    return;
                }
                subName += " (contains song)";
            }
            customListTableData.data.Add(new CustomCellInfo(playlist.collectionName, subName, playlist.coverImage));
        }

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            customListTableData.tableView.ClearSelection();
            // Folder Selected
            if (index < childManagers.Length)
            {
                ShowPlaylistsForManager(childManagers[index]);
            }
            else
            {
                index -= childManagers.Length;
                var selectedPlaylist = childPlaylists[index];
                if (HighlightDifficulty)
                {
                    selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap);
                }
                else
                {
                    selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
                }
                try
                {
                    parentManager.StorePlaylist(selectedPlaylist);
                    popupModalsController.ShowOkModal(modalTransform, string.Format("Song successfully added to {0}", selectedPlaylist.collectionName), null, animateDismiss: false);
                }
                catch (Exception e)
                {
                    popupModalsController.ShowOkModal(modalTransform, "An error occured while adding song to playlist.", null, animateDismiss: false);
                    Plugin.Log.Critical(string.Format("An exception was thrown while adding a song to a playlist.\nException Message: {0}", e.Message));
                }
                finally
                {
                    ShowPlaylistsForManager(parentManager);
                }
            }
        }

        [UIAction("back-button-pressed")]
        private void BackButtonPressed()
        {
            ShowPlaylistsForManager(parentManager.Parent);
        }

        [UIValue("folder-text")]
        private string FolderText
        {
            get => parentManager == null ? "" : Path.GetFileName(parentManager.PlaylistPath);
        }

        [UIValue("highlight-difficulty")]
        private bool HighlightDifficulty
        {
            get => PluginConfig.Instance.HighlightDifficulty;
            set
            {
                PluginConfig.Instance.HighlightDifficulty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightDifficulty)));
            }
        }

        [UIValue("back-active")]
        private bool BackActive
        {
            get => parentManager != null && parentManager.Parent != null;
        }

        #endregion

        #region Create Playlist

        [UIAction("open-keyboard")]
        private void OpenKeyboard()
        {
            popupModalsController.ShowKeyboard(modalTransform, CreatePlaylist, animateDismiss: false);
        }

        private void CreatePlaylist(string playlistName)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist;
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return;
            }
            if (!PluginConfig.Instance.DefaultImageDisabled)
            {
                playlist = PlaylistLibUtils.CreatePlaylist(playlistName, PluginConfig.Instance.AuthorName, parentManager);
            }
            else
            {
                playlist = PlaylistLibUtils.CreatePlaylist(playlistName, PluginConfig.Instance.AuthorName, "", parentManager);
            }
            if (playlist is IDeferredSpriteLoad deferredSpriteLoadPlaylist && !deferredSpriteLoadPlaylist.SpriteWasLoaded)
            {
                _ = playlist.coverImage;
                deferredSpriteLoadPlaylist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                deferredSpriteLoadPlaylist.SpriteLoaded += DeferredSpriteLoadPlaylist_SpriteLoaded;
            }
            childPlaylists.Add(playlist);
            customListTableData.tableView.ReloadData();
        }

        #endregion

    }
}
