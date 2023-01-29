using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using PlaylistManager.Downloaders;

namespace PlaylistManager.Managers
{
    internal class PlaylistUIManager : IInitializable, IDisposable, ILevelCollectionsTableUpdater
    {
        private readonly SelectLevelCategoryViewController selectLevelCategoryViewController;
        private readonly PlaylistSequentialDownloader playlistDownloader;

        private readonly List<ILevelCategoryUpdater> levelCategoryUpdaters;
        private readonly IPMRefreshable refreshable;

        public event Action<IAnnotatedBeatmapLevelCollection[], int>? LevelCollectionTableViewUpdatedEvent;

        internal PlaylistUIManager(SelectLevelCategoryViewController selectLevelCategoryViewController,
            PlaylistSequentialDownloader playlistDownloader, List<ILevelCategoryUpdater> levelCategoryUpdaters, IPMRefreshable refreshable)
        {
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.playlistDownloader = playlistDownloader;

            this.levelCategoryUpdaters = levelCategoryUpdaters;
            this.refreshable = refreshable;
        }

        public void Initialize()
        {
            // Whenever a level category is selected
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent += SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent += SelectLevelCategoryViewController_didDeactivateEvent;

            // Whenever a refresh is requested
            BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.PlaylistsRefreshRequested += PlaylistManager_PlaylistsRefreshRequested;
        }

        public void Dispose()
        {
            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent -= SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent -= SelectLevelCategoryViewController_didDeactivateEvent;
            
            BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.PlaylistsRefreshRequested -= PlaylistManager_PlaylistsRefreshRequested;
        }

        private void SelectLevelCategoryViewController_didSelectLevelCategoryEvent(SelectLevelCategoryViewController selectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            foreach (var levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(levelCategory, false);
            }
        }

        private void SelectLevelCategoryViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            foreach (var levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(selectLevelCategoryViewController.selectedLevelCategory, true);
            }
        }

        private void SelectLevelCategoryViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            foreach (var levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory.None, false);
            }
        }

        private void PlaylistManager_PlaylistsRefreshRequested(object sender, string requester)
        {
            Plugin.Log.Info("Playlist Refresh requested by: " + requester);
            refreshable.Refresh();
        }
    }
}
