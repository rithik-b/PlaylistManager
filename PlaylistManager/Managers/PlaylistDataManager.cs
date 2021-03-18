using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using Zenject;

namespace PlaylistManager
{
    public class PlaylistDataManager : IInitializable, IDisposable, ILevelCategoryUpdater
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        private readonly ILevelCollectionUpdater levelCollectionUpdater;
        private readonly List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;

        public BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist;
        public BeatSaberPlaylistsLib.Types.IPlaylistSong selectedPlaylistSong;
        public BeatSaberPlaylistsLib.PlaylistManager parentManager;

        internal PlaylistDataManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, ILevelCollectionUpdater levelCollectionUpdater, List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;

            this.levelCollectionUpdater = levelCollectionUpdater;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent += LevelCollectionTableView_DidSelectLevelEvent;
        }

        public void Dispose()
        {
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= AnnotatedBeatmapLevelCollectionsViewController_didSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent -= LevelCollectionTableView_DidSelectLevelEvent;
        }

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory _)
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
            levelCollectionUpdater.LevelCollectionUpdated(annotatedBeatmapLevelCollection, parentManager);
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
    }
}
