using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using System.Threading.Tasks;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;
using PlaylistManager.HarmonyPatches;
using System.Linq;

namespace PlaylistManager.Managers
{
    internal class PlaylistUIManager : IInitializable, IDisposable, ILevelCollectionsTableUpdater
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly SelectLevelCategoryViewController selectLevelCategoryViewController;
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;

        private readonly PlaylistDownloader playlistDownloader;
        private int downloadingBeatmapCollectionIdx;
        private IAnnotatedBeatmapLevelCollection[] downloadingBeatmapLevelCollections;

        private readonly List<ILevelCategoryUpdater> levelCategoryUpdaters;
        private readonly IPMRefreshable refreshable;
        private readonly IPlatformUserModel platformUserModel;

        public event Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;

        internal PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController, 
            StandardLevelDetailViewController standardLevelDetailViewController, PlaylistDownloader playlistDownloader, List<ILevelCategoryUpdater> levelCategoryUpdaters, IPMRefreshable refreshable, 
            IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.standardLevelDetailViewController = standardLevelDetailViewController;

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
            _ = AssignAuthor();
        }

        public void Dispose()
        {
            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent -= SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent -= SelectLevelCategoryViewController_didDeactivateEvent;

            playlistDownloader.QueueUpdatedEvent -= PlaylistDownloader_QueueUpdatedEvent;
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;

            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested -= PlaylistManager_PlaylistsRefreshRequested;
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
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && playlistDownloader.downloadQueue.Count == 0)
            {
                downloadingBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToArray();
                downloadingBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
                LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
            }
        }

        private void LevelFilteringNavigationController_SecondChildControllerUpdatedEvent()
        {
            LevelCollectionTableViewUpdatedEvent?.Invoke(downloadingBeatmapLevelCollections, downloadingBeatmapCollectionIdx);
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
        }

        private void PlaylistManager_PlaylistsRefreshRequested(object sender, string requester)
        {
            Plugin.Log.Info("Playlist Refresh requested by: " + requester);
            refreshable.Refresh();
        }

        private async Task AssignAuthor()
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
