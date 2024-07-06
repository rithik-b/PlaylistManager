using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;

/*
 * This patch removes the download icon for empty beatmaplevelcollections
 * Introduced since 1.18.0
 */

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.RefreshAvailabilityAsync))]
    internal class AnnotatedBeatmapLevelCollectionCell_RefreshAvailabilityAsync
    {
        private static void Postfix(AnnotatedBeatmapLevelCollectionCell __instance)
        {
            if (__instance._beatmapLevelPack is PlaylistLevelPack playlistLevelPack)
            {
                __instance.SetDownloadIconVisible(PluginConfig.Instance.ShowDownloadIcon && PlaylistLibUtils.GetMissingSongs(playlistLevelPack.playlist).Count > 0);
            }
        }
    }
}
