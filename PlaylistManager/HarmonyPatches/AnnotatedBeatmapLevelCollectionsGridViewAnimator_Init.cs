using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.Init))]
    internal class AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init
    {
        public static event Action OnGridViewInit;
        private static void Postfix()
        {
            OnGridViewInit?.Invoke();
        }
    }
}
