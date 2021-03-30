using IPA.Utilities;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using Zenject;

namespace PlaylistManager
{
    public class PlaylistDataManager : IInitializable, IDisposable
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly AnnotatedBeatmapLevelCollectionsTableView annotatedBeatmapLevelCollectionsTableView;
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly LevelFilteringNavigationController levelFilteringNavigationController;

        private readonly List<ILevelCollectionUpdater> levelCollectionUpdaters;
        private readonly List<ILevelCollectionsTableUpdater> levelCollectionsTableUpdaters;
        private readonly List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;

        public BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist;
        public BeatSaberPlaylistsLib.Types.IPlaylistSong selectedPlaylistSong;
        public BeatSaberPlaylistsLib.PlaylistManager parentManager;

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.Accessor AnnotatedBeatmapLevelCollectionsTableViewAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.GetAccessor("_annotatedBeatmapLevelCollectionsTableView");

        private readonly BeatmapLevelPack emptyBeatmapLevelPack;

        internal PlaylistDataManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelPackDetailViewController levelPackDetailViewController, LevelFilteringNavigationController levelFilteringNavigationController,
            List<ILevelCollectionUpdater> levelCollectionUpdaters, List<ILevelCollectionsTableUpdater> levelCollectionsTableUpdaters, List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            annotatedBeatmapLevelCollectionsTableView = AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController);
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.levelFilteringNavigationController = levelFilteringNavigationController;

            this.levelCollectionUpdaters = levelCollectionUpdaters;
            this.levelCollectionsTableUpdaters = levelCollectionsTableUpdaters;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;

            emptyBeatmapLevelPack = new BeatmapLevelPack(CustomLevelLoader.kCustomLevelPackPrefixId + "playlistmanager_empty", "Empty", "Empty", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite, new BeatmapLevelCollection(new IPreviewBeatmapLevel[0]));
        }

        public void Initialize()
        {
            levelPackDetailViewController.didActivateEvent += LevelPackDetailViewController_didActivateEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent += LevelCollectionTableView_DidSelectLevelEvent;

            foreach (ILevelCollectionsTableUpdater levelCollectionsTableUpdater in levelCollectionsTableUpdaters)
            {
                levelCollectionsTableUpdater.LevelCollectionTableViewUpdatedEvent += LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated;
            }
        }

        public void Dispose()
        {
            levelPackDetailViewController.didActivateEvent -= LevelPackDetailViewController_didActivateEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent -= LevelCollectionTableView_DidSelectLevelEvent;

            foreach (ILevelCollectionsTableUpdater levelCollectionsTableUpdater in levelCollectionsTableUpdaters)
            {
                levelCollectionsTableUpdater.LevelCollectionTableViewUpdatedEvent -= LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated;
            }
        }

        private void LevelPackDetailViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection);
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

        private void LevelCollectionsTableUpdater_LevelCollectionTableViewUpdated(IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections, int indexToSelect)
        {
            if (annotatedBeatmapLevelCollections.Length != 0)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollectionsTableView, annotatedBeatmapLevelCollections[indexToSelect]);
                levelFilteringNavigationController.HandleAnnotatedBeatmapLevelCollectionsViewControllerDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollections[indexToSelect]);
            }
            else
            {
                annotatedBeatmapLevelCollections = new IBeatmapLevelPack[1];
                annotatedBeatmapLevelCollections[0] = emptyBeatmapLevelPack;
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, 0, true);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollectionsTableView, annotatedBeatmapLevelCollections[0]);
                levelFilteringNavigationController.HandleAnnotatedBeatmapLevelCollectionsViewControllerDidSelectAnnotatedBeatmapLevelCollection(annotatedBeatmapLevelCollections[0]);
            }
        }
    }
}
