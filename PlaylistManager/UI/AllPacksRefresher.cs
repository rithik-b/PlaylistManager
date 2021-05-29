using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
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
            IBeatmapLevelPack[] annotatedBeatmapLevelCollections = Accessors.CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks.Concat(PlaylistLibUtils.playlistManager.GetAllPlaylists(true)).ToArray();
            int indexToSelect = annotatedBeatmapLevelCollections.IndexOf(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection);
            if (indexToSelect != -1)
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
            }
        }
    }
}
