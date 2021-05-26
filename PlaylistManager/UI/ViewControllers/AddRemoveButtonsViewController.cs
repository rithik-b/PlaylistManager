using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using PlaylistManager.Interfaces;
using System.Reflection;
using Zenject;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;
using System.ComponentModel;
using PlaylistManager.Utilities;

namespace PlaylistManager.UI
{
    public class AddRemoveButtonsViewController : IInitializable, IPreviewBeatmapLevelUpdater, ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private LevelCollectionTableView levelCollectionTableView;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly AddPlaylistModalController addPlaylistController;
        private readonly PopupModalsController popupModalsController;

        public event PropertyChangedEventHandler PropertyChanged;
        private IPreviewBeatmapLevel selectedBeatmapLevel;
        private BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private bool _addActive;
        private bool _removeActive;

        [UIComponent("root")]
        private RectTransform rootTransform;

        public AddRemoveButtonsViewController(StandardLevelDetailViewController standardLevelDetailViewController, LevelCollectionViewController levelCollectionViewController, LevelCollectionNavigationController levelCollectionNavigationController,
               AddPlaylistModalController addPlaylistController, PopupModalsController popupModalsController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            levelCollectionTableView = Accessors.LevelCollectionTableViewAccessor(ref levelCollectionViewController);
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.addPlaylistController = addPlaylistController;
            this.popupModalsController = popupModalsController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.AddRemoveButtonsView.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
            rootTransform.transform.localScale *= 0.7f;
            AddActive = false;
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
            levelCollectionTableView.SetData(selectedPlaylist.beatmapLevelCollection.beatmapLevels, Accessors.FavoriteLevelIdsAccessor(ref levelCollectionTableView), false);
            levelCollectionNavigationController.HideDetailViewController();
        }

        [UIValue("remove-active")]
        private bool RemoveActive
        {
            get => _removeActive;
            set
            {
                _removeActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemoveActive)));
            }
        }

        #endregion

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            selectedBeatmapLevel = beatmapLevel;
            if (beatmapLevel.levelID.EndsWith(" WIP"))
            {
                AddActive = false;
                RemoveActive = false;
            }
            else if (beatmapLevel is IPlaylistSong)
            {
                AddActive = true;
                RemoveActive = true;
            }
            else
            {
                AddActive = true;
                RemoveActive = false;
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
