using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using PlaylistManager.Utilities;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
    public class LevelFilteringNavigationController_ShowPacksInChildController
    {
        internal static event Action AllPacksViewSelectedEvent;

        internal static void Prefix(LevelFilteringNavigationController __instance, ref IReadOnlyList<BeatmapLevelPack> beatmapLevelPacks)
        {
            if (__instance._selectLevelCategoryViewController.selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                beatmapLevelPacks = beatmapLevelPacks.ToArray().AddRangeToArray(PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks());
                AllPacksViewSelectedEvent?.Invoke();
            }
        }
    }
}