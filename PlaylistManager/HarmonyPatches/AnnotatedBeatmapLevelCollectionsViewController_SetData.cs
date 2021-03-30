using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal static void Prefix(ref IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections)
        {
            // Check if annotatedBeatmapLevelCollections is empty (Versus Tab)
            if (annotatedBeatmapLevelCollections.Length == 0)
                return;
            // Checks if this is the playlists view
            if (annotatedBeatmapLevelCollections[0] is CustomBeatmapLevelPack)
            {
                // Check if this is caused by switching tabs
                StackTrace stack = new StackTrace();
                for (int i = 0; i < stack.FrameCount; i++)
                {
                    if (stack.GetFrame(i).GetMethod().Name == "ShowPacksInSecondChildController")
                    {
                        annotatedBeatmapLevelCollections = annotatedBeatmapLevelCollections.AddRangeToArray(PlaylistLibUtils.playlistManager.GetAllPlaylists(true));
                        return;
                    }
                }
            }
        }

        internal static void Postfix()
        {
            SetDataEvent?.Invoke();
        }
    }
}