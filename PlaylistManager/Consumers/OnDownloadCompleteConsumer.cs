using System;
using System.Linq;
using PlaylistManager.Downloaders;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using Zenject;

namespace PlaylistManager.Consumers
{
    internal class OnDownloadCompleteConsumer : ILevelCollectionsTableUpdater, IInitializable, IDisposable
    {
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly PlaylistSequentialDownloader playlistDownloader;
        
        private int? prevSelectedBeatmapCollectionIdx;
        private IAnnotatedBeatmapLevelCollection[]? selectedCategoryBeatmapLevelCollections;
        private IPreviewBeatmapLevel? prevSelectedBeatmap;
        
        public event Action<IAnnotatedBeatmapLevelCollection[], int>? LevelCollectionTableViewUpdatedEvent;

        public OnDownloadCompleteConsumer(LevelCollectionNavigationController levelCollectionNavigationController,
            AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, PlaylistSequentialDownloader playlistDownloader)
        {
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.playlistDownloader = playlistDownloader;
        }
        
        public void Initialize()
        {
            playlistDownloader.QueueUpdatedEvent += PlaylistDownloader_QueueUpdatedEvent;
        }

        public void Dispose()
        {
            playlistDownloader.QueueUpdatedEvent -= PlaylistDownloader_QueueUpdatedEvent;
            SongCore_RefreshLevelPacks.PacksToBeRefreshedEvent -= OnPacksToBeRefreshed;
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
        }

        private void PlaylistDownloader_QueueUpdatedEvent()
        {
            if (PlaylistSequentialDownloader.downloadQueue.Count == 0)
            {
                prevSelectedBeatmapCollectionIdx = null;
                selectedCategoryBeatmapLevelCollections = null;
                prevSelectedBeatmap = null;
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
                    selectedCategoryBeatmapLevelCollections = Accessors.AnnotatedBeatmapLevelCollectionsAccessor(ref annotatedBeatmapLevelCollectionsViewController).ToArray();
                    prevSelectedBeatmapCollectionIdx = annotatedBeatmapLevelCollectionsViewController.selectedItemIndex;
                }
                prevSelectedBeatmap = levelCollectionNavigationController.selectedBeatmapLevel;
                LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent += LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
            }
        }

        private void LevelFilteringNavigationController_SecondChildControllerUpdatedEvent()
        {
            LevelFilteringNavigationController_UpdateSecondChildControllerContent.SecondChildControllerUpdatedEvent -= LevelFilteringNavigationController_SecondChildControllerUpdatedEvent;
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && selectedCategoryBeatmapLevelCollections != null && prevSelectedBeatmapCollectionIdx != null)
            {
                LevelCollectionTableViewUpdatedEvent?.Invoke(selectedCategoryBeatmapLevelCollections, prevSelectedBeatmapCollectionIdx.Value);
            }
            if (levelCollectionNavigationController.isActiveAndEnabled && prevSelectedBeatmap != null)
            {
                levelCollectionNavigationController.SelectLevel(prevSelectedBeatmap);
            }
        }
    }
}