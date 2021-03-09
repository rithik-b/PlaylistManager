using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using PlaylistManager.Interfaces;
using System.Reflection;
using Zenject;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;
using System.ComponentModel;
using IPA.Utilities;
using PlaylistManager.Utilities;
using HMUI;
using System.Collections.Generic;

namespace PlaylistManager.UI
{
    public class AddRemoveButtonsViewController : IInitializable, IPreviewBeatmapLevelUpdater, IRefreshable, INotifyPropertyChanged
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private LevelCollectionTableView levelCollectionTableView;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly AddPlaylistViewController addPlaylistController;
        private readonly PopupModalsController popupModalsController;

        public event PropertyChangedEventHandler PropertyChanged;
        private IPreviewBeatmapLevel selectedBeatmapLevel;
        private bool _addActive;
        private bool _removeActive;

        public static readonly FieldAccessor<LevelCollectionViewController, LevelCollectionTableView>.Accessor LevelCollectionTableViewAccessor =
            FieldAccessor<LevelCollectionViewController, LevelCollectionTableView>.GetAccessor("_levelCollectionTableView");
        public static readonly FieldAccessor<LevelCollectionTableView, HashSet<string>>.Accessor FavoriteLevelIdsAccessor =
            FieldAccessor<LevelCollectionTableView, HashSet<string>>.GetAccessor("_favoriteLevelIds");
        public static readonly FieldAccessor<StandardLevelDetailViewController, IPreviewBeatmapLevel>.Accessor PreviewBeatmapLevelAccessor =
            FieldAccessor<StandardLevelDetailViewController, IPreviewBeatmapLevel>.GetAccessor("_previewBeatmapLevel");

        [UIComponent("root")]
        private RectTransform rootTransform;

        public AddRemoveButtonsViewController(StandardLevelDetailViewController standardLevelDetailViewController, LevelCollectionViewController levelCollectionViewController, LevelCollectionNavigationController levelCollectionNavigationController,
            AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, AddPlaylistViewController addPlaylistController, PopupModalsController popupModalsController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            levelCollectionTableView = LevelCollectionTableViewAccessor(ref levelCollectionViewController);
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
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
            addPlaylistController.ShowPlaylists();
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
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = (BeatSaberPlaylistsLib.Types.IPlaylist)annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
            selectedPlaylist.Remove((IPlaylistSong)selectedBeatmapLevel);
            PlaylistLibUtils.playlistManager.GetManagerForPlaylist(selectedPlaylist).StorePlaylist(selectedPlaylist);

            levelCollectionTableView.ClearSelection();
            levelCollectionTableView.SetData(selectedPlaylist.beatmapLevelCollection.beatmapLevels, FavoriteLevelIdsAccessor(ref levelCollectionTableView), false);
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

        public void Refresh()
        {
            PreviewBeatmapLevelUpdated(PreviewBeatmapLevelAccessor(ref standardLevelDetailViewController));
        }
    }
}
