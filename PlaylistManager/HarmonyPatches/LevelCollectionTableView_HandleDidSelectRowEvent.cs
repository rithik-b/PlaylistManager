using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionTableView), "HandleDidSelectRowEvent",
        new Type[] {
        typeof(TableView), typeof(int)})]
    public class LevelCollectionTableView_HandleDidSelectRowEvent
    {
        internal static event Action<IPreviewBeatmapLevel>? DidSelectLevelEvent;
        internal static void Prefix(int row, bool ____showLevelPackHeader)
        {
            if (____showLevelPackHeader)
            {
                row--;
            }

            if (row >= 0)
            {
                DidSelectLevelEvent?.Invoke(LevelCollectionViewController_SetData.beatmapLevels.ElementAt(row));
            }
        }
    }
}
