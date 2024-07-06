using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Linq;

namespace PlaylistManager.UI
{
    public class AllPacksRefresher : IPMRefreshable
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly BeatmapLevelsModel beatmapLevelsModel;
        private readonly PlaylistUpdater playlistUpdater;

        private AllPacksRefresher(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, BeatmapLevelsModel beatmapLevelsModel, PlaylistUpdater playlistUpdater)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.beatmapLevelsModel = beatmapLevelsModel;
            this.playlistUpdater = playlistUpdater;
        }

        public void Refresh()
        {
            var playlistLevelPacks = PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks();
            playlistUpdater.RefreshPlaylistChangedListeners(playlistLevelPacks);
            var annotatedBeatmapLevelCollections = beatmapLevelsModel._customLevelsRepository.beatmapLevelPacks.Concat(playlistLevelPacks).ToArray();
            var indexToSelect = Array.FindIndex(annotatedBeatmapLevelCollections, (pack) => pack.packID == annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack.packID);
            if (indexToSelect != -1)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
            }
        }
    }
}
