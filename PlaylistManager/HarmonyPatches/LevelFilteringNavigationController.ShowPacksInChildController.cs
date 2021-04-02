using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using PlaylistManager.Utilities;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController))]
    [HarmonyPatch("ShowPacksInSecondChildController", MethodType.Normal)]
    public class AnnotatedBeatmapLevelCollectionsViewController_SetData
    {
        internal static void Prefix(ref IReadOnlyList<IBeatmapLevelPack> beatmapLevelPacks)
        {
            // Check if annotatedBeatmapLevelCollections is empty (Versus Tab)
            if (beatmapLevelPacks.Count == 0)
                return;
            // Checks if this is the playlists view
            if (beatmapLevelPacks[0] is CustomBeatmapLevelPack)
            {
                beatmapLevelPacks = beatmapLevelPacks.ToArray().AddRangeToArray(PlaylistLibUtils.playlistManager.GetAllPlaylists(true));
            }
        }
    }
}