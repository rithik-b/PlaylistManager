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
        private static IAnnotatedBeatmapLevelCollection[] LoadedPlaylists
        {
            get
            {
                return (IAnnotatedBeatmapLevelCollection[]) PlaylistLibUtils.playlistManager.GetAllPlaylists(true);
            }
        }

        internal static IAnnotatedBeatmapLevelCollection[] otherCustomBeatmapLevelCollections;
        internal static bool isCustomBeatmapLevelPack = false;

        internal static void Prefix(ref IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections)
        {
            // Check if annotatedBeatmapLevelCollections is empty (Versus Tab)
            if (annotatedBeatmapLevelCollections.Length == 0)
                return;
            // Checks if this is the playlists view
            if (annotatedBeatmapLevelCollections[0] is CustomBeatmapLevelPack)
            {
                isCustomBeatmapLevelPack = true;
                IAnnotatedBeatmapLevelCollection[] allCustomBeatmapLevelCollections = new IAnnotatedBeatmapLevelCollection[LoadedPlaylists.Length + annotatedBeatmapLevelCollections.Length];
                otherCustomBeatmapLevelCollections = new IAnnotatedBeatmapLevelCollection[annotatedBeatmapLevelCollections.Length];
                for (int i = 0; i < annotatedBeatmapLevelCollections.Length; i++)
                {
                    allCustomBeatmapLevelCollections[i] = annotatedBeatmapLevelCollections[i];
                    otherCustomBeatmapLevelCollections[i] = annotatedBeatmapLevelCollections[i];
                }

                int j = 0;
                for (int i = annotatedBeatmapLevelCollections.Length; i < allCustomBeatmapLevelCollections.Length; i++)
                {
                    allCustomBeatmapLevelCollections[i] = LoadedPlaylists[j++];
                }

                annotatedBeatmapLevelCollections = allCustomBeatmapLevelCollections;
            }
            else
            {
                isCustomBeatmapLevelPack = false;
            }
        }
    }
}
