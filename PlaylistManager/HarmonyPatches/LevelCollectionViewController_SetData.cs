using System;
using System.Collections.Generic;
using HarmonyLib;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.SetData))]
    public class LevelCollectionViewController_SetData
    {
        internal static IReadOnlyCollection<BeatmapLevel> beatmapLevels { get; private set; }

        internal static void Prefix(BeatmapLevel[] beatmapLevels)
        {
            LevelCollectionViewController_SetData.beatmapLevels = beatmapLevels ?? Array.Empty<BeatmapLevel>();
        }
    }
}