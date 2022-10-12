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
using System.Threading.Tasks;
using PlaylistManager.Services;

namespace PlaylistManager.UI
{
    internal class AddPlaylistModalController : NotifiableBase
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly PlaylistsBrowserViewController playlistsBrowserViewController;
        private readonly PopupModalsController popupModalsController;
        private readonly PlaylistCreationService playlistCreationService;
        
        private bool parsed;

        [UIComponent("dropdown-options")]
        private readonly CustomListTableData dropdownTableData = null!;

        [UIComponent("highlight-checkbox")]
        private readonly RectTransform highlightCheckboxTransform = null!;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform = null!;

        private Vector3 modalPosition;

        [UIComponent("create-dropdown")]
        private ModalView createModal = null!;

        [UIComponent("create-dropdown")]
        private readonly RectTransform createModalTransform = null!;

        private Vector3 createModalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams = null!;

        public AddPlaylistModalController(StandardLevelDetailViewController standardLevelDetailViewController, PlaylistsBrowserViewController playlistsBrowserViewController, PopupModalsController popupModalsController, PlaylistCreationService playlistCreationService)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.playlistsBrowserViewController = playlistsBrowserViewController;
            this.popupModalsController = popupModalsController;
            this.playlistCreationService = playlistCreationService;
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
            playlistsBrowserViewController.ShowPlaylistBrowser(modalTransform, OnCellSelect);
        }

        private void OnCellSelect(IPlaylist selectedPlaylist)
        {
            var playlistSong = HighlightDifficulty ? selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap) :
                selectedPlaylist.Add(standardLevelDetailViewController.selectedDifficultyBeatmap.level);
            try
            {
                if (playlistSong == null)
                {
                    throw new Exception("PlaylistSong is null");
                }
                playlistsBrowserViewController.ParentManager.StorePlaylist(selectedPlaylist);
                popupModalsController.ShowOkModal(modalTransform,
                    $"Song successfully added to {selectedPlaylist.collectionName}", null, animateParentCanvas: false);
                Events.RaisePlaylistSongAdded(playlistSong, selectedPlaylist);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(modalTransform, "An error occured while adding song to playlist.", null, animateParentCanvas: false);
                Plugin.Log.Error($"An exception was thrown while adding a song to a playlist.\nException Message: {e.Message}");
            }
            finally
            {
                playlistsBrowserViewController.Refresh();
            }
        }

        [UIValue("highlight-difficulty")]
        private bool HighlightDifficulty
        {
            get => PluginConfig.Instance.HighlightDifficulty;
            set
            {
                PluginConfig.Instance.HighlightDifficulty = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Create Playlist

        [UIAction("select-option")]
        private void OnOptionSelect(TableView tableView, int index)
        {
            popupModalsController.ShowKeyboard(modalTransform,
                index == 0 ? playlistName => _ = CreatePlaylistAsync(playlistName) : CreateFolder,
                animateParentCanvas: false);
            tableView.ClearSelection();
            parserParams.EmitEvent("close-dropdown");
        }

        private async Task CreatePlaylistAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return;
            }

            await playlistCreationService.CreatePlaylistAsync(playlistName, playlistsBrowserViewController.ParentManager);
            playlistsBrowserViewController.Refresh();
        }

        private void CreateFolder(string folderName)
        {
            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            
            if (string.IsNullOrEmpty(folderName))
            {
                return;
            }

            BeatSaberPlaylistsLib.PlaylistManager childManager;
            try
            {
                childManager = playlistsBrowserViewController.ParentManager.CreateChildManager(folderName);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(modalTransform, "An error occured while creating a folder.",
                    null, animateParentCanvas: false);
                Plugin.Log.Error($"An exception was thrown while adding a song to a playlist.\nException Message: {e.Message}");
                return;
            }

            if (playlistsBrowserViewController.ChildManagers?.Contains(childManager) ?? false)
            {
                popupModalsController.ShowOkModal(modalTransform, "\"" + folderName + "\" already exists! Please use a different name.", null, animateParentCanvas: false);
            }
            else
            {
                playlistsBrowserViewController.Refresh();
                BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.RequestRefresh("PlaylistManager (plugin)");
            }
        }

        #endregion
    }
}
