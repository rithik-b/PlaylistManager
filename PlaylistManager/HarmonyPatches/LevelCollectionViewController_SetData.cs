using System;
using System.Collections.Generic;
using HarmonyLib;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.SetData))]
    public class LevelCollectionViewController_SetData
    {
        internal static IReadOnlyCollection<BeatmapLevel> beatmapLevels { get; private set; }

        private static void Prefix(BeatmapLevel[] beatmapLevels)
        {
            LevelCollectionViewController_SetData.beatmapLevels = beatmapLevels ?? Array.Empty<BeatmapLevel>();
        }

        private static void Postfix(LevelCollectionViewController __instance)
        {
            if (beatmapLevels.Count == 0)
            {
                __instance._levelCollectionTableView.gameObject.SetActive(true);
            }
        }
    }
}