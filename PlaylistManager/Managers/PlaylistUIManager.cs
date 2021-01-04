using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Zenject;
using PlaylistManager.Interfaces;

namespace PlaylistManager.Managers
{
    class PlaylistUIManager : IInitializable, IDisposable
    {
        AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        List<ILevelCollectionUpdater> levelCollectionUpdaters;

        LevelSelectionNavigationController levelSelectionNavigationController;
        ILevelPackUpdater levelPackUpdater;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, List<ILevelCollectionUpdater> levelCollectionUpdaters, LevelSelectionNavigationController levelSelectionNavigationController, ILevelPackUpdater levelPackUpdater)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionUpdaters = levelCollectionUpdaters;
            this.levelSelectionNavigationController = levelSelectionNavigationController;
            this.levelPackUpdater = levelPackUpdater;
        }

        public void Dispose()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            levelSelectionNavigationController.didSelectLevelPackEvent += LevelSelectionNavigationController_didSelectLevelPackEvent;
        }

        private void LevelSelectionNavigationController_didSelectLevelPackEvent(LevelSelectionNavigationController arg1, IBeatmapLevelPack arg2)
        {
            levelPackUpdater.LevelPackUpdated();
        }

        private void AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(IAnnotatedBeatmapLevelCollection obj)
        {
            foreach (ILevelCollectionUpdater levelCollectionUpdater in levelCollectionUpdaters)
            {
                levelCollectionUpdater.LevelCollectionUpdated(obj);
            }
        }
    }
}
