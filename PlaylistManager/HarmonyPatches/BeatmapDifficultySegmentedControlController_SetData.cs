using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDifficultySegmentedControlController), nameof(BeatmapDifficultySegmentedControlController.SetData))]
    public class BeatmapDifficultySegmentedControlController_SetData
    {
        internal static event Action? CharacteristicsSegmentedControllerDataSetEvent;
        static void Postfix()
        {
            CharacteristicsSegmentedControllerDataSetEvent?.Invoke();
        }
    }
}
