using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), "SetData",
        new Type[] {
        typeof(IReadOnlyList<IAnnotatedBeatmapLevelCollection>), typeof(int), typeof(bool)})]
    public class PlaylistCollectionOverride
    {
        internal static IAnnotatedBeatmapLevelCollection[] otherCustomBeatmapLevelCollections;
        internal static void Prefix(ref IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections)
        {
            // Check if annotatedBeatmapLevelCollections is empty (Versus Tab)
            if (annotatedBeatmapLevelCollections.Length == 0)
                return;
            // Checks if this is the playlists view
            if (annotatedBeatmapLevelCollections[0] is CustomBeatmapLevelPack)
            {
                otherCustomBeatmapLevelCollections = new IAnnotatedBeatmapLevelCollection[2];
                otherCustomBeatmapLevelCollections[0] = annotatedBeatmapLevelCollections[0];
                otherCustomBeatmapLevelCollections[1] = annotatedBeatmapLevelCollections[1];
            }
        }
    }
}
