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
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Components;
using PlaylistManager.Configuration;
using PlaylistManager.Services;

namespace PlaylistManager.UI
{
    internal class LevelDetailButtonsViewController : NotifiableBase, IInitializable, IDisposable, IPreviewBeatmapLevelUpdater, ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly LevelCollectionTableView levelCollectionTableView;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly AddPlaylistModalController addPlaylistController;
        private readonly PopupModalsController popupModalsController;
        private readonly DifficultyHighlighter difficultyHighlighter;
        private readonly AuthorNameService authorNameService;

        private IPreviewBeatmapLevel? selectedBeatmapLevel;
        private IPlaylist? selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager? parentManager;
        private bool _addActive;
        private bool _isPlaylistSong;
        private bool selectedDifficultyHighlighted;

        [UIComponent("root")]
        private readonly RectTransform rootTransform = null!;

        public LevelDetailButtonsViewController(StandardLevelDetailViewController standardLevelDetailViewController, LevelCollectionViewController levelCollectionViewController, LevelCollectionNavigationController levelCollectionNavigationController,
               AddPlaylistModalController addPlaylistController, PopupModalsController popupModalsController, DifficultyHighlighter difficultyHighlighter, AuthorNameService authorNameService)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            levelCollectionTableView = Accessors.LevelCollectionTableViewAccessor(ref levelCollectionViewController);
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.addPlaylistController = addPlaylistController;
            this.popupModalsController = popupModalsController;
            this.difficultyHighlighter = difficultyHighlighter;
            this.authorNameService = authorNameService;
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
                NotifyPropertyChanged();
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
            if (selectedPlaylist == null || parentManager == null || selectedBeatmapLevel == null)
                return;
            
            selectedPlaylist.Remove((IPlaylistSong)selectedBeatmapLevel);
            try
            {
                parentManager.StorePlaylist(selectedPlaylist);
                Events.RaisePlaylistSongRemoved((IPlaylistSong)selectedBeatmapLevel, selectedPlaylist);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(standardLevelDetailViewController.transform, "An error occured while removing a song from the playlist.", null);
                Plugin.Log.Critical($"An exception was thrown while adding a song to a playlist.\nException Message: {e.Message}");
            }

            levelCollectionTableView.ClearSelection();
            _ = ResetPlaylist();
        }

        private async Task ResetPlaylist()
        {
            if (PluginConfig.Instance.EasterEggs)
            {
                var authorName = await authorNameService.GetNameAsync();
                var cutieList = new[] {"GOOBIE", "ERIS", "PINK", "CANDL3"};

                if (cutieList.Any(n => authorName.ToUpper().Contains(n)))
                {
                    levelCollectionNavigationController.SetDataForPack(selectedPlaylist, true, true, $"{authorName} Cute");
                    return;
                }

                if (authorName.ToUpper().Contains("JOSHABI"))
                {
                    levelCollectionNavigationController.SetDataForPack(selectedPlaylist, true, true, "*Sneeze*");
                    return;
                }
            }
            
            levelCollectionNavigationController.SetDataForPack(selectedPlaylist, true, true, "Play");
        }

        #endregion

        #region Highlight Difficulty Button

        [UIAction("highlight-button-click")]
        private void HighlightButtonClick()
        {
            difficultyHighlighter.ToggleSelectedDifficultyHighlight();
            parentManager!.StorePlaylist(selectedPlaylist!);
            selectedDifficultyHighlighted = !selectedDifficultyHighlighted;
            NotifyPropertyChanged(nameof(HighlightButtonText));
            NotifyPropertyChanged(nameof(HighlightButtonHover));
        }

        private void DifficultyHighlighter_selectedDifficultyChanged(bool selectedDifficultyHighlighted)
        {
            this.selectedDifficultyHighlighted = selectedDifficultyHighlighted;
            NotifyPropertyChanged(nameof(HighlightButtonText));
            NotifyPropertyChanged(nameof(HighlightButtonHover));
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
                NotifyPropertyChanged();
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
            else if (beatmapLevel is IPlaylistSong && selectedPlaylist is { ReadOnly: false })
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

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager? parentManager)
        {
            if (annotatedBeatmapLevelCollection is IPlaylist selectedPlaylist)
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
