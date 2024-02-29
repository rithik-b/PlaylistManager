using System;
using System.Collections.Generic;
using HarmonyLib;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController))]
    [HarmonyPatch("SetData", MethodType.Normal)]
    public class LevelCollectionViewController_SetData
    {
        internal static IReadOnlyCollection<BeatmapLevel> beatmapLevels { get; private set; }
        
        internal static void Prefix(ref BeatmapLevel[] beatmapLevels)
        {
            LevelCollectionViewController_SetData.beatmapLevels = beatmapLevels ?? Array.Empty<BeatmapLevel>();
        }
    }
}