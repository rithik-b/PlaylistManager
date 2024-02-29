using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using PlaylistManager.Utilities;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController))]
    [HarmonyPatch("ShowPacksInSecondChildController", MethodType.Normal)]
    public class LevelFilteringNavigationController_ShowPacksInChildController
    {
        internal static event Action AllPacksViewSelectedEvent;

        internal static void Prefix(ref IReadOnlyList<BeatmapLevelPack> beatmapLevelPacks, ref SelectLevelCategoryViewController ____selectLevelCategoryViewController)
        {
            if (____selectLevelCategoryViewController.selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                beatmapLevelPacks = beatmapLevelPacks.ToArray().AddRangeToArray(PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks());
                AllPacksViewSelectedEvent?.Invoke();
            }
        }
    }
}