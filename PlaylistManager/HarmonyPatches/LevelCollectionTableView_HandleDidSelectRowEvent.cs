using HarmonyLib;
using System;
using System.Linq;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.HandleDidSelectRowEvent))]
    public class LevelCollectionTableView_HandleDidSelectRowEvent
    {
        internal static event Action<BeatmapLevel> DidSelectLevelEvent;
        internal static void Prefix(LevelCollectionTableView __instance, int row)
        {
            if (__instance._showLevelPackHeader)
            {
                row--;
            }

            if (row >= 0)
            {
                DidSelectLevelEvent?.Invoke(__instance._beatmapLevels.ElementAt(row));
            }
        }
    }
}
