using HarmonyLib;
using System;
using System.Linq;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.HandleDidSelectCellWithIndex))]
    public class LevelCollectionTableView_HandleDidSelectRowEvent
    {
        internal static event Action<BeatmapLevel> DidSelectLevelEvent;
        internal static void Prefix(LevelCollectionTableView __instance, int index)
        {
            if (__instance._showLevelPackHeader)
            {
                index--;
            }

            if (index >= 0)
            {
                DidSelectLevelEvent?.Invoke(__instance._beatmapLevels.ElementAt(index));
            }
        }
    }
}
