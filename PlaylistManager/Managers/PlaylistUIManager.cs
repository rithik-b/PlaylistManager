using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;
using PlaylistManager.HarmonyPatches;
using System.Linq;
using PlaylistManager.UI;

namespace PlaylistManager.Managers
{
    internal class PlaylistUIManager : IInitializable, IDisposable, ILevelCollectionsTableUpdater
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly SelectLevelCategoryViewController selectLevelCategoryViewController;
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly SettingsViewController settingsViewController;
        private readonly PlaylistDownloader playlistDownloader;

        private int downloadingBeatmapCollectionIdx;
        private IAnnotatedBeatmapLevelCollection[] downloadingBeatmapLevelCollections;
        private IPreviewBeatmapLevel downloadingBeatmap;

        private readonly List<ILevelCategoryUpdater> levelCategoryUpdaters;
        private readonly IPMRefreshable refreshable;
        private readonly IPlatformUserModel platformUserModel;

        public event Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;

        internal PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionNavigationController levelCollectionNavigationController,
            SelectLevelCategoryViewController selectLevelCategoryViewController, StandardLevelDetailViewController standardLevelDetailViewController, SettingsViewController settingsViewController,
            PlaylistDownloader playlistDownloader, List<ILevelCategoryUpdater> levelCategoryUpdaters, IPMRefreshable refreshable, IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.settingsViewController = settingsViewController;
            this.playlistDownloader = playlistDownloader;

            this.levelCategoryUpdaters = levelCategoryUpdaters;
            this.refreshable = refreshable;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            // Whenever a level category is selected
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent += SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent += SelectLevelCategoryViewController_didDeactivateEvent;

            // When all playlists finish downloading
            playlistDownloader.QueueUpdatedEvent += PlaylistDownloader_QueueUpdatedEvent;

            // Whenever a refresh is requested
            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested += PlaylistManager_PlaylistsRefreshRequested;

            // For assigning playlist author
            settingsViewController.NameFetchRequestedEvent += AssignAuthor;
            AssignAuthor();
        }

        public void Dispose()
        {
            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent -= SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent -= SelectLevelCategoryViewController_didDeactivateEvent;

            playlistDownloader.QueueUpdatedEvent -= PlaylistDownloader_QueueUpdatedEvent;
            SongCore_RefreshLevelPacks.PacksToBeRefreshedEvent -= OnPacksToBeRefreshed;
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;

            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested -= PlaylistManager_PlaylistsRefreshRequested;

            settingsViewController.NameFetchRequestedEvent -= AssignAuthor;
        }

        private void SelectLevelCategoryViewController_didSelectLevelCategoryEvent(SelectLevelCategoryViewController selectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            foreach (ILevelCategoryUpdater levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(levelCategory, false);
            }
        }

        private void SelectLevelCategoryViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            foreach (ILevelCategoryUpdater levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(selectLevelCategoryViewController.selectedLevelCategory, true);
            }
        }

        private void SelectLevelCategoryViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            foreach (ILevelCategoryUpdater levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory.None, false);
            }
        }

        private void PlaylistDownloader_QueueUpdatedEvent()
        {
            if (PlaylistDownloader.downloadQueue.Count == 0)
            {
                SongCore_RefreshLevelPacks.PacksToBeRefreshedEvent += OnPacksToBeRefreshed;
            }
        }

        private void OnPacksToBeRefreshed()
        {
            SongCore_RefreshLevelPacks.PacksToBeRefreshedEvent -= OnPacksToBeRefreshed;

            if (levelCollectionNavigationController.isActiveAndEnabled)
            {
                if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled)
                {
                    downloadingBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToArray();
                    downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
                }
                downloadingBeatmap = levelCollectionNavigationController.selectedBeatmapLevel;
                LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
            }
        }

        private void LevelFilteringNavigationController_SecondChildControllerUpdatedEvent()
        {
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled)
            {
                LevelCollectionTableViewUpdatedEvent?.Invoke(downloadingBeatmapLevelCollections, downloadingBeatmapCollectionIdx);
            }
            if (levelCollectionNavigationController.isActiveAndEnabled && downloadingBeatmap != null)
            {
                levelCollectionNavigationController.SelectLevel(downloadingBeatmap);
            }
        }

        private void PlaylistManager_PlaylistsRefreshRequested(object sender, string requester)
        {
            Plugin.Log.Info("Playlist Refresh requested by: " + requester);
            refreshable.Refresh();
        }

        private async void AssignAuthor()
        {
            if (PluginConfig.Instance.AutomaticAuthorName)
            {
                UserInfo user = await platformUserModel.GetUserInfo();
                if (PluginConfig.Instance.AuthorName == null && user == null)
                {
                    PluginConfig.Instance.AuthorName = nameof(PlaylistManager);
                }
                else
                {
                    PluginConfig.Instance.AuthorName = user?.userName ?? PluginConfig.Instance.AuthorName;
                }
            }
            else
            {
                PluginConfig.Instance.AuthorName = PluginConfig.Instance.AuthorName;
            }
        }
    }
}
