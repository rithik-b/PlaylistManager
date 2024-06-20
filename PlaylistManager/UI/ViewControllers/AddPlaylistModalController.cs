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
using IPA.Loader;
using SiraUtil.Zenject;

namespace PlaylistManager.UI
{
    public class AddPlaylistModalController : INotifyPropertyChanged
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private readonly PluginMetadata pluginMetadata;
        private readonly BSMLParser bsmlParser;

        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private List<BeatSaberPlaylistsLib.PlaylistManager> childManagers;
        private List<IPlaylist> childPlaylists;

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

        public AddPlaylistModalController(StandardLevelDetailViewController standardLevelDetailViewController, PopupModalsController popupModalsController, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.popupModalsController = popupModalsController;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;
            folderIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.FolderIcon.png");
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.AddPlaylistModal.bsml"), standardLevelDetailViewController._standardLevelDetailView.gameObject, this);
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

            createModal._animateParentCanvas = false;
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

            foreach (var playlistManager in childManagers)
            {
                playlistTableData.data.Add(new CustomCellInfo(Path.GetFileName(playlistManager.PlaylistPath), "Folder", folderIcon));
            }
            foreach (var playlist in childPlaylists)
            {
                if (!playlist.SmallSpriteWasLoaded)
                {
                    playlist.SpriteLoaded -= StagedSpriteLoadPlaylist_SpriteLoaded;
                    playlist.SpriteLoaded += StagedSpriteLoadPlaylist_SpriteLoaded;
                    _ = playlist.SmallSprite;
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

        private void StagedSpriteLoadPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is IStagedSpriteLoad stagedSpriteLoadPlaylist)
            {
                if (parentManager.GetAllPlaylists(false).Contains((IPlaylist)stagedSpriteLoadPlaylist))
                {
                    ShowPlaylist((IPlaylist)stagedSpriteLoadPlaylist);
                }
                playlistTableData.tableView.ReloadDataKeepingPosition();
                stagedSpriteLoadPlaylist.SpriteLoaded -= StagedSpriteLoadPlaylist_SpriteLoaded;
            }
        }

        private void ShowPlaylist(IPlaylist playlist)
        {
            var subName = string.Format("{0} songs", playlist.BeatmapLevels.Length);
            if (playlist.BeatmapLevels.Any(level => level.levelID == standardLevelDetailViewController.beatmapKey.levelId))
            {
                if (!playlist.AllowDuplicates)
                {
                    childPlaylists.Remove(playlist);
                    return;
                }
                subName += " (contains song)";
            }
            playlistTableData.data.Add(new CustomCellInfo(playlist.Title, subName, playlist.SmallSprite));
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
                    playlistSong = selectedPlaylist.Add(standardLevelDetailViewController.beatmapLevel, standardLevelDetailViewController.beatmapKey);
                }
                else
                {
                    playlistSong = selectedPlaylist.Add(standardLevelDetailViewController.beatmapLevel);
                }
                try
                {
                    selectedPlaylist.RaisePlaylistChanged();
                    parentManager.StorePlaylist(selectedPlaylist);
                    popupModalsController.ShowOkModal(modalTransform, string.Format("Song successfully added to {0}", selectedPlaylist.Title), null, animateParentCanvas: false);
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

            var playlist = PlaylistLibUtils.CreatePlaylistWithConfig(playlistName, parentManager);

            if (playlist is IDeferredSpriteLoad deferredSpriteLoadPlaylist && !deferredSpriteLoadPlaylist.SpriteWasLoaded)
            {
                deferredSpriteLoadPlaylist.SpriteLoaded -= StagedSpriteLoadPlaylist_SpriteLoaded;
                deferredSpriteLoadPlaylist.SpriteLoaded += StagedSpriteLoadPlaylist_SpriteLoaded;
                _ = playlist.Sprite;
            }

            childPlaylists.Add(playlist);
            playlistTableData.tableView.ReloadDataKeepingPosition();
        }

        private void CreateFolder(string folderName)
        {
            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                var childManager = parentManager.CreateChildManager(folderName);

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
