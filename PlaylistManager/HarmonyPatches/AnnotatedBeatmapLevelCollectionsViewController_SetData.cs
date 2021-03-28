using System;
using System.Collections.Generic;
using HarmonyLib;
using PlaylistManager.Utilities;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), "SetData",
        new Type[] {
        typeof(IReadOnlyList<IAnnotatedBeatmapLevelCollection>), typeof(int), typeof(bool)})]
    public class AnnotatedBeatmapLevelCollectionsViewController_SetData
    {
        internal static event Action SetDataEvent;

        internal static void Postfix()
        {
            SetDataEvent?.Invoke();
        }
    }
}