using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using PlaylistManager.Interfaces;
using System.Reflection;
using Zenject;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;
using System.ComponentModel;
using PlaylistManager.Utilities;
using System;
using PlaylistManager.Configuration;

namespace PlaylistManager.UI
{
    public class LevelDetailButtonsViewController : IInitializable, IDisposable, IPreviewBeatmapLevelUpdater, ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private LevelCollectionTableView levelCollectionTableView;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly AddPlaylistModalController addPlaylistController;
        private readonly PopupModalsController popupModalsController;
        private readonly DifficultyHighlighter difficultyHighlighter;

        public event PropertyChangedEventHandler PropertyChanged;
        private IPreviewBeatmapLevel selectedBeatmapLevel;
        private BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private bool _addActive;
        private bool _isPlaylistSong;
        private bool selectedDifficultyHighlighted;

        [UIComponent("root")]
        private RectTransform rootTransform;

        public LevelDetailButtonsViewController(StandardLevelDetailViewController standardLevelDetailViewController, LevelCollectionViewController levelCollectionViewController, LevelCollectionNavigationController levelCollectionNavigationController,
               AddPlaylistModalController addPlaylistController, PopupModalsController popupModalsController, DifficultyHighlighter difficultyHighlighter)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            levelCollectionTableView = Accessors.LevelCollectionTableViewAccessor(ref levelCollectionViewController);
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.addPlaylistController = addPlaylistController;
            this.popupModalsController = popupModalsController;
            this.difficultyHighlighter = difficultyHighlighter;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.LevelDetailButtonsView.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
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
            if (selectedBeatmapLevel is IPlaylistSong)
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
            selectedPlaylist.Remove((IPlaylistSong)selectedBeatmapLevel);
            parentManager.StorePlaylist(selectedPlaylist);

            levelCollectionTableView.ClearSelection();
            if (PluginConfig.Instance.AuthorName.ToUpper().Contains("GOOBIE"))
            {
                levelCollectionNavigationController.SetDataForPack(selectedPlaylist, true, true, true, "Goobie Cute");
            }
            else
            {
                levelCollectionNavigationController.SetDataForPack(selectedPlaylist, true, true, true, "Play");
            }

            levelCollectionNavigationController.HideDetailViewController();
        }

        #endregion

        #region Highlight Difficulty Button

        [UIAction("highlight-button-click")]
        private void HighlightButtonClick()
        {
            difficultyHighlighter.ToggleSelectedDifficultyHighlight();
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

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            selectedBeatmapLevel = beatmapLevel;
            if (beatmapLevel.levelID.EndsWith(" WIP"))
            {
                AddActive = false;
                IsPlaylistSong = false;
            }
            else if (beatmapLevel is IPlaylistSong)
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

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (annotatedBeatmapLevelCollection is BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist)
            {
                this.selectedPlaylist = selectedPlaylist;
                this.parentManager = parentManager;
            }
            else
            {
                this.selectedPlaylist = null;
                this.parentManager = null;
            }
        }
    }
}
