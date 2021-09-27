using HarmonyLib;

/*
 * This patch removes the download icon for empty beatmaplevelcollections
 * Introduced since 1.18.0
 */

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.RefreshAvailabilityAsync))]
    internal class AnnotatedBeatmapLevelCollectionCell_RefreshAvailabilityAsync
    {
        private static void Postfix(AnnotatedBeatmapLevelCollectionCell __instance, IAnnotatedBeatmapLevelCollection ____annotatedBeatmapLevelCollection)
        {
            if (____annotatedBeatmapLevelCollection is BeatSaberPlaylistsLib.Types.IPlaylist)
            {
                __instance.SetDownloadIconVisible(false);
            }
        }
    }
}
