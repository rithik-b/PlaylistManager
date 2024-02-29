using System;
using System.Collections.Generic;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.UI;
using PlaylistManager.Utilities;
using UnityEngine;
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

            emptyBeatmapLevelPack = new BeatmapLevelPack(CustomLevelLoader.kCustomLevelPackPrefixId + "CustomLevels", "Custom Levels", "Custom Levels", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite, BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite, Array.Empty<BeatmapLevel>(), PlayerSensitivityFlag.Safe);
        }

        public void Initialize()
        {
            levelPackDetailViewController.didActivateEvent += LevelPackDetailViewController_didActivateEvent;
            levelFilteringNavigationController.didSelectBeatmapLevelPackEvent += LevelFilteringNavigationController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent += LevelCollectionTableView_DidSelectLevelEvent;

            if (foldersViewController != null)
            {
                foldersViewController.ParentManagerUpdatedEvent += FoldersViewController_ParentManagerUpdatedEvent;
            }

            foreach (var levelCollectionsTableUpdater in levelCollectionsTableUpdaters)
            {
                levelCollectionsTableUpdater.LevelCollectionTableViewUpdatedEvent += LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated;
            }
        }

        public void Dispose()
        {
            levelPackDetailViewController.didActivateEvent -= LevelPackDetailViewController_didActivateEvent;
            levelFilteringNavigationController.didSelectBeatmapLevelPackEvent -= LevelFilteringNavigationController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent -= LevelCollectionTableView_DidSelectLevelEvent;

            if (foldersViewController != null)
            {
                foldersViewController.ParentManagerUpdatedEvent -= FoldersViewController_ParentManagerUpdatedEvent;
            }

            foreach (var levelCollectionsTableUpdater in levelCollectionsTableUpdaters)
            {
                levelCollectionsTableUpdater.LevelCollectionTableViewUpdatedEvent -= LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated;
            }
        }

        private void LevelPackDetailViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled)
            {
                AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack);
            }
        }

        private void LevelFilteringNavigationController_didSelectAnnotatedBeatmapLevelCollectionEvent(LevelFilteringNavigationController controller, BeatmapLevelPack annotatedBeatmapLevelCollection, GameObject noDataInfoPrefab, LevelSelectionOptions levelSelectionOptions)
        {
            AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(annotatedBeatmapLevelCollection);
        }

        private void AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(BeatmapLevelPack beatmapLevelPack)
        {
            if (beatmapLevelPack is PlaylistLevelPack selectedPlaylist)
            {
                Events.RaisePlaylistSelected(selectedPlaylist.playlist, parentManager);
                this.selectedPlaylist = selectedPlaylist.playlist;
                parentManager = PlaylistLibUtils.playlistManager.GetManagerForPlaylist(selectedPlaylist.playlist);
            }
            else
            {
                this.selectedPlaylist = null;
                parentManager = null;
            }
            foreach (var levelCollectionUpdater in levelCollectionUpdaters)
            {
                levelCollectionUpdater.LevelCollectionUpdated(beatmapLevelPack, parentManager);
            }
        }

        private void LevelCollectionTableView_DidSelectLevelEvent(BeatmapLevel previewBeatmapLevel)
        {
            if (previewBeatmapLevel is PlaylistLevel playlistLevel)
            {
                Events.RaisePlaylistSongSelected(playlistLevel.playlistSong);
                selectedPlaylistSong = playlistLevel.playlistSong;
            }
            else
            {
                selectedPlaylistSong = null;
            }
            foreach (var previewBeatmapLevelUpdater in previewBeatmapLevelUpdaters)
            {
                previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(previewBeatmapLevel);
            }
        }

        private void FoldersViewController_ParentManagerUpdatedEvent(BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            foreach (var parentManagerUpdater in parentManagerUpdaters)
            {
                parentManagerUpdater.ParentManagerUpdated(parentManager);
            }
        }

        private void LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated(IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections, int indexToSelect)
        {
            if (annotatedBeatmapLevelCollections.Count != 0)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                levelFilteringNavigationController.HandleAnnotatedBeatmapLevelCollectionsViewControllerDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollections[indexToSelect]);
            }
            else
            {
                annotatedBeatmapLevelCollections = new BeatmapLevelPack[] { emptyBeatmapLevelPack };
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, 0, true);
                levelFilteringNavigationController.HandleAnnotatedBeatmapLevelCollectionsViewControllerDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollections[0]);
            }
        }
    }
}
