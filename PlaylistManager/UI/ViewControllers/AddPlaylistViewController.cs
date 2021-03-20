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

namespace PlaylistManager.UI
{
    public class AddPlaylistViewController : INotifyPropertyChanged
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly PopupModalsController popupModalsController;

        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private BeatSaberPlaylistsLib.PlaylistManager[] childManagers;
        private BeatSaberPlaylistsLib.Types.IPlaylist[] childPlaylists;

        private readonly Sprite folderIcon;
        private bool parsed;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        [UIComponent("back-rect")]
        private readonly RectTransform backTransform;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public AddPlaylistViewController(StandardLevelDetailViewController standardLevelDetailViewController, PopupModalsController popupModalsController)
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
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.AddPlaylistView.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
                modalPosition = modalTransform.position; // Position can change if SongBrowser is clicked while modal is opened so storing here
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
            childPlaylists = parentManager.GetAllPlaylists(false);

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

            if (parentManager.Parent != null)
            {
                backTransform.gameObject.SetActive(true);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
            }
            else
            {
                backTransform.gameObject.SetActive(false);
            }
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
                selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
                try
                {
                    parentManager.StorePlaylist(selectedPlaylist);
                    popupModalsController.ShowOkModal(modalTransform, string.Format("Song successfully added to {0}", selectedPlaylist.collectionName), null);
                }
                catch (Exception e)
                {
                    popupModalsController.ShowOkModal(modalTransform, "An error occured while adding song to playlist.", null);
                    Plugin.Log.Critical(string.Format("An exception was thrown while adding a song to a playlist.\nException Message: {0}", e.Message));
                }
                finally
                {
                    string subName = string.Format("{0} songs", selectedPlaylist.beatmapLevelCollection.beatmapLevels.Length);
                    if (Array.Exists(selectedPlaylist.beatmapLevelCollection.beatmapLevels, level => level.levelID == standardLevelDetailViewController.selectedDifficultyBeatmap.level.levelID))
                    {
                        if (!selectedPlaylist.AllowDuplicates)
                        {
                            customListTableData.data.RemoveAt(index);
                        }
                        else
                        {
                            subName += " (contains song)";
                            customListTableData.data[index] = new CustomCellInfo(selectedPlaylist.collectionName, subName, selectedPlaylist.coverImage);
                        }
                    }
                    customListTableData.tableView.RefreshCellsContent();
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

        #endregion

        [UIAction("open-keyboard")]
        private void OpenKeyboard()
        {
            popupModalsController.ShowKeyboard(modalTransform, CreatePlaylist);
        }

        private void CreatePlaylist(string playlistName)
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
            ShowPlaylistsForManager(parentManager);
        }
    }
}
