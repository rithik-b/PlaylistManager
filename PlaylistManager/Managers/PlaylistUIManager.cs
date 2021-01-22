using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using PlaylistManager.UI;

namespace PlaylistManager.Managers
{
    class PlaylistUIManager : IInitializable, IDisposable
    {
        AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        PlaylistViewController playlistViewController;
        ILevelCollectionUpdater levelCollectionUpdater;

        LevelCollectionViewController levelCollectionViewController;
        List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, PlaylistViewController playlistViewController, ILevelCollectionUpdater levelCollectionUpdater, LevelCollectionViewController levelCollectionViewController, List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.playlistViewController = playlistViewController;
            this.levelCollectionUpdater = levelCollectionUpdater;
            this.levelCollectionViewController = levelCollectionViewController;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;
        }

        public void Dispose()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            levelCollectionViewController.didSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            levelCollectionViewController.didSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;
        }

        private void LevelCollectionViewController_didSelectLevelEvent(LevelCollectionViewController levelCollectionViewController, IPreviewBeatmapLevel beatmapLevel)
        {
            foreach (IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater in previewBeatmapLevelUpdaters)
            {
                previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(beatmapLevel);
            }
        }

        private void DidSelectAnnotatedBeatmapLevelCollectionEvent(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection)
        {
            levelCollectionUpdater.LevelCollectionUpdated(annotatedBeatmapLevelCollection);
        }
    }
}
