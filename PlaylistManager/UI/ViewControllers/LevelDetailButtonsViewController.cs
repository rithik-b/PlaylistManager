using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using PlaylistManager.Interfaces;
using Zenject;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;
using System.ComponentModel;
using PlaylistManager.Utilities;
using System;
using IPA.Loader;
using PlaylistManager.Configuration;
using SiraUtil.Zenject;

namespace PlaylistManager.UI
{
    public class LevelDetailButtonsViewController : IInitializable, IDisposable, IBeatmapLevelUpdater, ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private LevelCollectionTableView levelCollectionTableView;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly AddPlaylistModalController addPlaylistController;
        private readonly PopupModalsController popupModalsController;
        private readonly DifficultyHighlighter difficultyHighlighter;
        private readonly PluginMetadata pluginMetadata;
        private readonly BSMLParser bsmlParser;

        public event PropertyChangedEventHandler PropertyChanged;
        private BeatmapLevel selectedBeatmapLevel;
        private IPlaylist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private bool _addActive;
        private bool _isPlaylistSong;
        private bool selectedDifficultyHighlighted;

        [UIComponent("root")]
        private RectTransform rootTransform;

        public LevelDetailButtonsViewController(StandardLevelDetailViewController standardLevelDetailViewController, LevelCollectionViewController levelCollectionViewController, LevelCollectionNavigationController levelCollectionNavigationController,
               AddPlaylistModalController addPlaylistController, PopupModalsController popupModalsController, DifficultyHighlighter difficultyHighlighter, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            levelCollectionTableView = levelCollectionViewController._levelCollectionTableView;
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.addPlaylistController = addPlaylistController;
            this.popupModalsController = popupModalsController;
            this.difficultyHighlighter = difficultyHighlighter;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;
        }

        public void Initialize()
        {
            bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.LevelDetailButtonsView.bsml"), standardLevelDetailViewController._standardLevelDetailView.gameObject, this);
            rootTransform.transform.localScale *= 0.7f;
            AddActive = false;
            difficultyHighlighter.selectedDifficultyChanged += DifficultyHighlighter_selectedDifficultyChanged;
        }

        public void Dispose()
        {
            difficultyHighlighter.selectedDifficultyChanged -= DifficultyHighlighter_selectedDifficultyChanged;
        }

        #region Add Button

        [UIAction("add-button-click")]
        private void OpenAddModal()
        {
            addPlaylistController.ShowModal();
        }

        [UIValue("add-active")]
        private bool AddActive
        {
            get => _addActive;
            set
            {
                _addActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddActive)));
            }
        }

        #endregion

        #region Remove Button

        [UIAction("remove-button-click")]
        private void DisplayRemoveWarning()
        {
            if (selectedBeatmapLevel is PlaylistLevel)
            {
                popupModalsController.ShowYesNoModal(standardLevelDetailViewController.transform, string.Format("Are you sure you would like to remove {0} from the playlist?", selectedBeatmapLevel.songName), RemoveSong);
            }
            else
            {
                popupModalsController.ShowOkModal(standardLevelDetailViewController.transform, "Error: The selected song is not part of a playlist.", null);
            }
        }

        private void RemoveSong()
        {
            if (selectedBeatmapLevel is not PlaylistLevel playlistLevel)
            {
                return;
            }

            selectedPlaylist.Remove(playlistLevel.playlistSong);

            try
            {
                selectedPlaylist.RaisePlaylistChanged();
                parentManager.StorePlaylist(selectedPlaylist);
                Events.RaisePlaylistSongRemoved(playlistLevel.playlistSong, selectedPlaylist);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(standardLevelDetailViewController.transform, "An error occured while removing a song from the playlist.", null);
                Plugin.Log.Critical(string.Format("An exception was thrown while adding a song to a playlist.\nException Message: {0}", e.Message));
            }

            levelCollectionTableView.ClearSelection();

            // The cutie list
            if ((PluginConfig.Instance.AuthorName.IndexOf("GOOBIE", StringComparison.OrdinalIgnoreCase) >= 0 || PluginConfig.Instance.AuthorName.IndexOf("ERIS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 PluginConfig.Instance.AuthorName.IndexOf("PINK", StringComparison.OrdinalIgnoreCase) >= 0 || PluginConfig.Instance.AuthorName.IndexOf("CANDL3", StringComparison.OrdinalIgnoreCase) >= 0) && PluginConfig.Instance.EasterEggs)
            {
                levelCollectionNavigationController.SetDataForPack(selectedPlaylist.PlaylistLevelPack, true, true, $"{PluginConfig.Instance.AuthorName} Cute", false);
            }
            else if (PluginConfig.Instance.AuthorName.IndexOf("JOSHABI", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                levelCollectionNavigationController.SetDataForPack(selectedPlaylist.PlaylistLevelPack, true, true, $"*Sneeze*", false);
            }
            else
            {
                levelCollectionNavigationController.SetDataForPack(selectedPlaylist.PlaylistLevelPack, true, true, "Play", false);
            }

            levelCollectionNavigationController.HideDetailViewController();
        }

        #endregion

        #region Highlight Difficulty Button

        [UIAction("highlight-button-click")]
        private void HighlightButtonClick()
        {
            difficultyHighlighter.ToggleSelectedDifficultyHighlight();
            selectedPlaylist.RaisePlaylistChanged();
            parentManager.StorePlaylist(selectedPlaylist);
            selectedDifficultyHighlighted = !selectedDifficultyHighlighted;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightButtonText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightButtonHover)));
        }

        private void DifficultyHighlighter_selectedDifficultyChanged(bool selectedDifficultyHighlighted)
        {
            this.selectedDifficultyHighlighted = selectedDifficultyHighlighted;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightButtonText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightButtonHover)));
        }

        [UIValue("highlight-button-text")]
        private string HighlightButtonText => selectedDifficultyHighlighted ? "⬛" : "⬜";

        [UIValue("highlight-button-hover")]
        private string HighlightButtonHover => selectedDifficultyHighlighted ? "Unhighlight selected difficulty" : "Highlight selected difficulty";

        #endregion

        [UIValue("playlist-song")]
        private bool IsPlaylistSong
        {
            get => _isPlaylistSong;
            set
            {
                _isPlaylistSong = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaylistSong)));
            }
        }

        public void BeatmapLevelUpdated(BeatmapLevel beatmapLevel)
        {
            selectedBeatmapLevel = beatmapLevel;
            if (beatmapLevel.levelID.EndsWith(" WIP"))
            {
                AddActive = false;
                IsPlaylistSong = false;
            }
            else if (beatmapLevel is PlaylistLevel && selectedPlaylist is { ReadOnly: false })
            {
                AddActive = true;
                IsPlaylistSong = true;
            }
            else
            {
                AddActive = true;
                IsPlaylistSong = false;
            }
        }

        public void LevelCollectionUpdated(BeatmapLevelPack annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (annotatedBeatmapLevelCollection is PlaylistLevelPack playlistLevelPack)
            {
                selectedPlaylist = playlistLevelPack.playlist;
                this.parentManager = parentManager;
            }
            else
            {
                selectedPlaylist = null;
                this.parentManager = null;
            }
        }
    }
}
