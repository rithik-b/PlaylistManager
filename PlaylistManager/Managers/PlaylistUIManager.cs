﻿using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using PlaylistManager.UI;
using PlaylistManager.HarmonyPatches;

namespace PlaylistManager.Managers
{
    internal class PlaylistUIManager : IInitializable, IDisposable
    {
        AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        SelectLevelCategoryViewController selectLevelCategoryViewController;
        PlaylistViewController playlistViewController;
        ILevelCollectionUpdater levelCollectionUpdater;

        List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;

        StandardLevelDetailViewController standardLevelDetailViewController;
        List<IPlaylistManagerModal> playlistManagerModals;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController, PlaylistViewController playlistViewController, ILevelCollectionUpdater levelCollectionUpdater,
            List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters, StandardLevelDetailViewController standardLevelDetailViewController, List<IPlaylistManagerModal> playlistManagerModals)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.playlistViewController = playlistViewController;
            this.levelCollectionUpdater = levelCollectionUpdater;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.playlistManagerModals = playlistManagerModals;
        }

        public void Dispose()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.didSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.didSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;
            standardLevelDetailViewController.didDeactivateEvent += StandardLevelDetailViewController_didDeactivateEvent;
            standardLevelDetailViewController.didChangeContentEvent += StandardLevelDetailViewController_didChangeContentEvent;
        }

        private void LevelCollectionViewController_didSelectLevelEvent(IPreviewBeatmapLevel beatmapLevel)
        {
            foreach (IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater in previewBeatmapLevelUpdaters)
            {
                previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(beatmapLevel);
            }
        }

        private void DidSelectAnnotatedBeatmapLevelCollectionEvent(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection)
        {
            levelCollectionUpdater.LevelCollectionUpdated();
        }

        private void SelectLevelCategoryViewController_didSelectLevelCategoryEvent(SelectLevelCategoryViewController arg1, SelectLevelCategoryViewController.LevelCategory arg2)
        {
            levelCollectionUpdater.LevelCollectionUpdated();
        }

        private void StandardLevelDetailViewController_didChangeContentEvent(StandardLevelDetailViewController standardLevelDetailViewController, StandardLevelDetailViewController.ContentType contentType)
        {
            if (contentType != StandardLevelDetailViewController.ContentType.OwnedAndReady)
            {
                foreach (IPlaylistManagerModal playlistManagerModal in playlistManagerModals)
                {
                    playlistManagerModal.ParentControllerDeactivated();
                }
            }
        }

        private void StandardLevelDetailViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            foreach (IPlaylistManagerModal playlistManagerModal in playlistManagerModals)
            {
                playlistManagerModal.ParentControllerDeactivated();
            }
        }
    }
}