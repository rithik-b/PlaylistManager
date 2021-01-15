using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using PlaylistManager.HarmonyPatches;

namespace PlaylistManager.Managers
{
    class PlaylistUIManager : IInitializable, IDisposable
    {
        AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        List<ILevelCollectionUpdater> levelCollectionUpdaters;

        LevelCollectionViewController levelCollectionViewController;
        IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, List<ILevelCollectionUpdater> levelCollectionUpdaters, LevelCollectionViewController levelCollectionViewController, IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionUpdaters = levelCollectionUpdaters;
            this.levelCollectionViewController = levelCollectionViewController;
            this.previewBeatmapLevelUpdater = previewBeatmapLevelUpdater;
        }

        public void Dispose()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            levelCollectionViewController.didSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            levelCollectionViewController.didSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;
        }

        private void LevelCollectionViewController_didSelectLevelEvent(LevelCollectionViewController levelCollectionViewController, IPreviewBeatmapLevel beatmapLevel)
        {
            previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(beatmapLevel);
        }

        private void AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection)
        {
            foreach (ILevelCollectionUpdater levelCollectionUpdater in levelCollectionUpdaters)
            {
                levelCollectionUpdater.LevelCollectionUpdated(annotatedBeatmapLevelCollection);
            }
        }
    }
}
