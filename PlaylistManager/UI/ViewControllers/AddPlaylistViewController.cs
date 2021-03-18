using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using System.Reflection;
using HMUI;
using PlaylistManager.Utilities;
using UnityEngine;
using PlaylistManager.Configuration;
using BeatSaberPlaylistsLib.Types;
using Zenject;
using BeatSaberMarkupLanguage.Parser;

namespace PlaylistManager.UI
{
    public class AddPlaylistViewController : IInitializable, IDisposable
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly PopupModalsController popupModalsController;

        private BeatSaberPlaylistsLib.Types.IPlaylist[] loadedplaylists;
        private bool parsed;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public AddPlaylistViewController(StandardLevelDetailViewController standardLevelDetailViewController, PopupModalsController popupModalsController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.popupModalsController = popupModalsController;
            parsed = false;
        }

        public void Initialize()
        {
            standardLevelDetailViewController.didDeactivateEvent += ParentControllerDeactivated;
        }

        public void Dispose()
        {
            standardLevelDetailViewController.didDeactivateEvent -= ParentControllerDeactivated;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.AddPlaylistView.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
                modalPosition = modalTransform.position; // Position can change if SongBrowser is clicked while modal is opened so storing here
                parsed = true;
            }
        }

        #region Show Playlists
        internal void ShowPlaylists()
        {
            Parse();
            parserParams.EmitEvent("open-modal");
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
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
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

        #endregion

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            var selectedPlaylist = loadedplaylists[index];
            selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
            customListTableData.tableView.ClearSelection();
            try
            {
                PlaylistLibUtils.playlistManager.GetManagerForPlaylist(selectedPlaylist).StorePlaylist(selectedPlaylist);
                popupModalsController.ShowOkModal(modalTransform, string.Format("Song successfully added to {0}", selectedPlaylist.collectionName), null);
            }
            catch(Exception e)
            {
                popupModalsController.ShowOkModal(modalTransform, "An error occured while adding song to playlist.", null);
                Plugin.Log.Critical(string.Format("An exception was thrown while adding a song to a playlist.\nException Message: {0}", e.Message));
            }
            finally
            {
                string subName = string.Format("{0} songs", selectedPlaylist.beatmapLevelCollection.beatmapLevels.Length);
                if (Array.Exists(selectedPlaylist.beatmapLevelCollection.beatmapLevels, level => level.levelID == standardLevelDetailViewController.selectedDifficultyBeatmap.level.levelID))
                {
                    subName += " (contains song)";
                }
                customListTableData.data[index] = new CustomCellInfo(selectedPlaylist.collectionName, subName, selectedPlaylist.coverImage);
                customListTableData.tableView.RefreshCellsContent();
            }
        }

        [UIAction("open-keyboard")]
        private void OpenKeyboard()
        {
            popupModalsController.ShowKeyboard(rootTransform, CreatePlaylist);
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
            ShowPlaylists();
        }

        public void ParentControllerDeactivated(bool removedFromHierarchy, bool screenSystemDisabling)
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
