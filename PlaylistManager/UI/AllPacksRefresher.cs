using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Linq;

namespace PlaylistManager.UI
{
    public class AllPacksRefresher : IPMRefreshable
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private BeatmapLevelsModel beatmapLevelsModel;

        public AllPacksRefresher(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, BeatmapLevelsModel beatmapLevelsModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.beatmapLevelsModel = beatmapLevelsModel;
        }

        public void Refresh()
        {
            var annotatedBeatmapLevelCollections = beatmapLevelsModel._customLevelsRepository.beatmapLevelPacks.Concat(PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks()).ToArray();
            var indexToSelect = Array.FindIndex(annotatedBeatmapLevelCollections, (pack) => pack.packID == annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack.packID);
            if (indexToSelect != -1)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
            }
        }
    }
}
