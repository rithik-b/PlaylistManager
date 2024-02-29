using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Interfaces;
using System;
using System.Collections.Generic;

namespace PlaylistManager.UI
{
    internal class CoverImageUpdater : ILevelCollectionUpdater
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController beatmapLevelCollectionsViewController;

        private IPlaylist selectedPlaylist;

        public CoverImageUpdater(LevelPackDetailViewController levelPackDetailViewController, LevelCollectionNavigationController levelCollectionNavigationController, AnnotatedBeatmapLevelCollectionsViewController beatmapLevelCollectionsViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.beatmapLevelCollectionsViewController = beatmapLevelCollectionsViewController;
        }

        private void SelectedPlaylist_PlaylistChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void SelectedPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            // TODO: this should really apply to any playlist that's modified while the UI is open, not just the selected one.
            // TODO: calling RaisePlaylistChanged everywhere kind of sucks. is there a better way to do this?
            var pack = selectedPlaylist.PlaylistLevelPack;

            var previousCollections = beatmapLevelCollectionsViewController._annotatedBeatmapLevelCollections;
            var collections = CloneAndOverwriteEntry(previousCollections, pack);
            beatmapLevelCollectionsViewController._annotatedBeatmapLevelCollections = collections;
            beatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._annotatedBeatmapLevelCollections = collections;
            beatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._gridView.ReloadData();

            levelPackDetailViewController.SetData(pack);
            levelPackDetailViewController.ShowContent(LevelPackDetailViewController.ContentType.Owned);

            levelCollectionNavigationController._levelPack = pack;

            var levelCollectionTableView = levelCollectionNavigationController._levelCollectionViewController._levelCollectionTableView;
            levelCollectionTableView._headerText = pack.packName;
            levelCollectionTableView._tableView.RefreshCellsContent();
        }

        public void LevelCollectionUpdated(BeatmapLevelPack annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (selectedPlaylist != null)
            {
                selectedPlaylist.PlaylistChanged -= SelectedPlaylist_PlaylistChanged;
                selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }

            if (annotatedBeatmapLevelCollection is PlaylistLevelPack playlistLevelPack)
            {
                selectedPlaylist = playlistLevelPack.playlist;
                selectedPlaylist.PlaylistChanged += SelectedPlaylist_PlaylistChanged;
                selectedPlaylist.SpriteLoaded += SelectedPlaylist_SpriteLoaded;
            }
            else
            {
                selectedPlaylist = null;
            }
        }

        private static IReadOnlyList<BeatmapLevelPack> CloneAndOverwriteEntry(IReadOnlyList<BeatmapLevelPack> original, BeatmapLevelPack item)
        {
            BeatmapLevelPack[] beatmapLevelPackCollection = new BeatmapLevelPack[original.Count];

            for (int i = 0; i < beatmapLevelPackCollection.Length; ++i)
            {
                if (original[i].packID == item.packID)
                {
                    beatmapLevelPackCollection[i] = item;
                }
                else
                {
                    beatmapLevelPackCollection[i] = original[i];
                }
            }

            return beatmapLevelPackCollection;
        }
    }
}
