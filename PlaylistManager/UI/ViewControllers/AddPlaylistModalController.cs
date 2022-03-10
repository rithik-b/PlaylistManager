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
        private List<BeatSaberPlaylistsLib.PlaylistManager> childManagers;
        private List<BeatSaberPlaylistsLib.Types.IPlaylist> childPlaylists;

        private readonly Sprite folderIcon;
        private bool parsed;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("list")]
        public CustomListTableData playlistTableData;

        [UIComponent("dropdown-options")]
        public CustomListTableData dropdownTableData;

        [UIComponent("highlight-checkbox")]
        private readonly RectTransform highlightCheckboxTransform;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIComponent("create-dropdown")]
        private ModalView createModal;

        [UIComponent("create-dropdown")]
        private readonly RectTransform createModalTransform;

        private Vector3 createModalPosition;

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
                modalPosition = modalTransform.localPosition;
                createModalPosition = createModalTransform.localPosition;
            }
            modalTransform.localPosition = modalPosition; // Reset position
            createModalTransform.localPosition = createModalPosition;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            parsed = true;
            highlightCheckboxTransform.transform.localScale *= 0.5f;

            Accessors.AnimateCanvasAccessor(ref createModal) = false;
            dropdownTableData.data.Add(new CustomCellInfo("Playlist"));
            dropdownTableData.data.Add(new CustomCellInfo("Folder"));
            dropdownTableData.tableView.ReloadData();
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
            playlistTableData.data.Clear();

            this.parentManager = parentManager;
            childManagers = parentManager.GetChildManagers().ToList();
            var childPlaylists = parentManager.GetAllPlaylists(false).Where(playlist => !playlist.ReadOnly);
            this.childPlaylists = childPlaylists.ToList();

            foreach (BeatSaberPlaylistsLib.PlaylistManager playlistManager in childManagers)
            {
                playlistTableData.data.Add(new CustomCellInfo(Path.GetFileName(playlistManager.PlaylistPath), "Folder", folderIcon));
            }
            foreach (IPlaylist playlist in childPlaylists)
            {
                if (playlist is IDeferredSpriteLoad deferredSpriteLoadPlaylist && !deferredSpriteLoadPlaylist.SpriteWasLoaded)
                {
                    deferredSpriteLoadPlaylist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                    deferredSpriteLoadPlaylist.SpriteLoaded += DeferredSpriteLoadPlaylist_SpriteLoaded;
                    _ = playlist.coverImage;
                }
                else
                {
                    ShowPlaylist(playlist);
                }
            }
            playlistTableData.tableView.ReloadData();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
        }

        private void DeferredSpriteLoadPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is IDeferredSpriteLoad deferredSpriteLoadPlaylist)
            {
                if (parentManager.GetAllPlaylists(false).Contains((IPlaylist)deferredSpriteLoadPlaylist))
                {
                    ShowPlaylist((IPlaylist)deferredSpriteLoadPlaylist);
                }
                playlistTableData.tableView.ReloadDataKeepingPosition();
                (deferredSpriteLoadPlaylist).SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
            }
        }

        private void ShowPlaylist(IPlaylist playlist)
        {
            string subName = string.Format("{0} songs", playlist.beatmapLevelCollection.beatmapLevels.Count);
            if (playlist.beatmapLevelCollection.beatmapLevels.Any(level => level.levelID == standardLevelDetailViewController.selectedDifficultyBeatmap.level.levelID))
            {
                if (!playlist.AllowDuplicates)
                {
                    childPlaylists.Remove(playlist);
                    return;
                }
                subName += " (contains song)";
            }
            playlistTableData.data.Add(new CustomCellInfo(playlist.collectionName, subName, playlist.coverImage));
        }

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            playlistTableData.tableView.ClearSelection();
            // Folder Selected
            if (index < childManagers.Count)
            {
                ShowPlaylistsForManager(childManagers[index]);
            }
            else
            {
                index -= childManagers.Count;
                var selectedPlaylist = childPlaylists[index];
                IPlaylistSong playlistSong;
                if (HighlightDifficulty)
                {
                    playlistSong = selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap);
                }
                else
                {
                    playlistSong = selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
                }
                try
                {
                    parentManager.StorePlaylist(selectedPlaylist);
                    popupModalsController.ShowOkModal(modalTransform, string.Format("Song successfully added to {0}", selectedPlaylist.collectionName), null, animateParentCanvas: false);
                    Events.RaisePlaylistSongAdded(playlistSong, selectedPlaylist);
                }
                catch (Exception e)
                {
                    popupModalsController.ShowOkModal(modalTransform, "An error occured while adding song to playlist.", null, animateParentCanvas: false);
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

        [UIAction("select-option")]
        private void OnOptionSelect(TableView tableView, int index)
        {
            popupModalsController.ShowKeyboard(modalTransform, index == 0 ? CreatePlaylist : CreateFolder, animateParentCanvas: false);
            tableView.ClearSelection();
            parserParams.EmitEvent("close-dropdown");
        }

        private void CreatePlaylist(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return;
            }

            BeatSaberPlaylistsLib.Types.IPlaylist playlist = PlaylistLibUtils.CreatePlaylistWithConfig(playlistName, parentManager);

            if (playlist is IDeferredSpriteLoad deferredSpriteLoadPlaylist && !deferredSpriteLoadPlaylist.SpriteWasLoaded)
            {
                deferredSpriteLoadPlaylist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                deferredSpriteLoadPlaylist.SpriteLoaded += DeferredSpriteLoadPlaylist_SpriteLoaded;
                _ = playlist.coverImage;
            }

            childPlaylists.Add(playlist);
            playlistTableData.tableView.ReloadDataKeepingPosition();
        }

        private void CreateFolder(string folderName)
        {
            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                BeatSaberPlaylistsLib.PlaylistManager childManager = parentManager.CreateChildManager(folderName);

                if (childManagers.Contains(childManager))
                {
                    popupModalsController.ShowOkModal(modalTransform, "\"" + folderName + "\" already exists! Please use a different name.", null, animateParentCanvas: false);
                }
                else
                {
                    playlistTableData.data.Insert(childManagers.Count, new CustomCellInfo(Path.GetFileName(childManager.PlaylistPath), "Folder", folderIcon));
                    playlistTableData.tableView.ReloadDataKeepingPosition();
                    childManagers.Add(childManager);
                    PlaylistLibUtils.playlistManager.RequestRefresh("PlaylistManager (plugin)");
                }
            }
        }

        #endregion
    }
}
