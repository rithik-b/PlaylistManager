using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using PlaylistManager.Utilities;
using System;
using System.Collections.ObjectModel;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController))]
    [HarmonyPatch("ShowPacksInSecondChildController", MethodType.Normal)]
    public class LevelFilteringNavigationController_ShowPacksInChildController
    {
        internal static event Action? AllPacksViewSelectedEvent;
        internal static void Prefix(ref IReadOnlyList<IBeatmapLevelPack> beatmapLevelPacks, ref SelectLevelCategoryViewController ____selectLevelCategoryViewController)
        {
            if (____selectLevelCategoryViewController.selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                var playlists = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true);
                beatmapLevelPacks = beatmapLevelPacks.Concat(playlists).ToArray();
                AllPacksViewSelectedEvent?.Invoke();
            }
        }
    }
}