using IPA.Utilities;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.UI;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using Zenject;

namespace PlaylistManager
{
    public class PlaylistDataManager : IInitializable, IDisposable
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly LevelFilteringNavigationController levelFilteringNavigationController;
        private readonly FoldersViewController foldersViewController;

        private readonly List<ILevelCollectionUpdater> levelCollectionUpdaters;
        private readonly List<ILevelCollectionsTableUpdater> levelCollectionsTableUpdaters;
        private readonly List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;
        private readonly List<IParentManagerUpdater> parentManagerUpdaters;

        public BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist;
        public BeatSaberPlaylistsLib.Types.IPlaylistSong selectedPlaylistSong;
        public BeatSaberPlaylistsLib.PlaylistManager parentManager;

        private readonly BeatmapLevelPack emptyBeatmapLevelPack;

        internal PlaylistDataManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelPackDetailViewController levelPackDetailViewController, LevelFilteringNavigationController levelFilteringNavigationController,
            [InjectOptional] FoldersViewController foldersViewController, List<ILevelCollectionUpdater> levelCollectionUpdaters, List<ILevelCollectionsTableUpdater> levelCollectionsTableUpdaters, List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters,
            List<IParentManagerUpdater> parentManagerUpdaters)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.levelFilteringNavigationController = levelFilteringNavigationController;
            this.foldersViewController = foldersViewController;
            this.parentManagerUpdaters = parentManagerUpdaters;

            this.levelCollectionUpdaters = levelCollectionUpdaters;
            this.levelCollectionsTableUpdaters = levelCollectionsTableUpdaters;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;

            emptyBeatmapLevelPack = new BeatmapLevelPack(CustomLevelLoader.kCustomLevelPackPrefixId + "CustomLevels", "Custom Levels", "Custom Levels", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite, BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite, new BeatmapLevelCollection(new IPreviewBeatmapLevel[0]));
        }

        public void Initialize()
        {
            levelPackDetailViewController.didActivateEvent += LevelPackDetailViewController_didActivateEvent;
            levelFilteringNavigationController.didSelectAnnotatedBeatmapLevelCollectionEvent += LevelFilteringNavigationController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent += LevelCollectionTableView_DidSelectLevelEvent;

            if (foldersViewController != null)
            {
                foldersViewController.ParentManagerUpdatedEvent += FoldersViewController_ParentManagerUpdatedEvent;
            }

            foreach (ILevelCollectionsTableUpdater levelCollectionsTableUpdater in levelCollectionsTableUpdaters)
            {
                levelCollectionsTableUpdater.LevelCollectionTableViewUpdatedEvent += LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated;
            }
        }

        public void Dispose()
        {
            levelPackDetailViewController.didActivateEvent -= LevelPackDetailViewController_didActivateEvent;
            levelFilteringNavigationController.didSelectAnnotatedBeatmapLevelCollectionEvent -= LevelFilteringNavigationController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent -= LevelCollectionTableView_DidSelectLevelEvent;

            if (foldersViewController != null)
            {
                foldersViewController.ParentManagerUpdatedEvent -= FoldersViewController_ParentManagerUpdatedEvent;
            }

            foreach (ILevelCollectionsTableUpdater levelCollectionsTableUpdater in levelCollectionsTableUpdaters)
            {
                levelCollectionsTableUpdater.LevelCollectionTableViewUpdatedEvent -= LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated;
            }
        }

        private void LevelPackDetailViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled)
            {
                AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection);
            }
        }

        private void LevelFilteringNavigationController_didSelectAnnotatedBeatmapLevelCollectionEvent(LevelFilteringNavigationController _, IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, UnityEngine.GameObject __, BeatmapCharacteristicSO ___)
        {
            AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(annotatedBeatmapLevelCollection);
        }

        private void AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection)
        {
            if (annotatedBeatmapLevelCollection is BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist)
            {
                Events.RaisePlaylistSelected(selectedPlaylist, parentManager);
                this.selectedPlaylist = selectedPlaylist;
                parentManager = PlaylistLibUtils.playlistManager.GetManagerForPlaylist(selectedPlaylist);
            }
            else
            {
                this.selectedPlaylist = null;
                parentManager = null;
            }
            foreach (ILevelCollectionUpdater levelCollectionUpdater in levelCollectionUpdaters)
            {
                levelCollectionUpdater.LevelCollectionUpdated(annotatedBeatmapLevelCollection, parentManager);
            }
        }

        private void LevelCollectionTableView_DidSelectLevelEvent(IPreviewBeatmapLevel previewBeatmapLevel)
        {
            if (previewBeatmapLevel is BeatSaberPlaylistsLib.Types.IPlaylistSong selectedPlaylistSong)
            {
                Events.RaisePlaylistSongSelected(selectedPlaylistSong);
                this.selectedPlaylistSong = selectedPlaylistSong;
            }
            else
            {
                this.selectedPlaylistSong = null;
            }
            foreach (IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater in previewBeatmapLevelUpdaters)
            {
                previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(previewBeatmapLevel);
            }
        }

        private void FoldersViewController_ParentManagerUpdatedEvent(BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            foreach (IParentManagerUpdater parentManagerUpdater in parentManagerUpdaters)
            {
                parentManagerUpdater.ParentManagerUpdated(parentManager);
            }
        }

        private void LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated(IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections, int indexToSelect)
        {
            if (annotatedBeatmapLevelCollections.Length != 0)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                levelFilteringNavigationController.HandleAnnotatedBeatmapLevelCollectionsViewControllerDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollections[indexToSelect]);
            }
            else
            {
                annotatedBeatmapLevelCollections = new IBeatmapLevelPack[1];
                annotatedBeatmapLevelCollections[0] = emptyBeatmapLevelPack;
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, 0, true);
                levelFilteringNavigationController.HandleAnnotatedBeatmapLevelCollectionsViewControllerDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollections[0]);
            }
        }
    }
}
