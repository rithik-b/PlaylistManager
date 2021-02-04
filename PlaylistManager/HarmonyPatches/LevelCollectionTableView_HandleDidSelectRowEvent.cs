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
    /*
     * Unwraps IPlaylistSong (if selected) and returns an IPreviewBeatmapLevel
     * Also has an event with the unwrapped PlaylistSong (or BeatmapLevel) for PlaylistManager to use
     * Any playlist GUI mod that loads playlist songs into a LevelCollectionTable MUST use this patcher so other mods remain compatible
     */

    [HarmonyPatch(typeof(LevelCollectionTableView), "HandleDidSelectRowEvent",
        new Type[] {
        typeof(TableView), typeof(int)})]
    public class LevelCollectionTableView_HandleDidSelectRowEvent
    {
        internal static event Action<IPreviewBeatmapLevel> DidSelectLevelEvent;
        internal static readonly MethodInfo _unwrapPlaylistSong = SymbolExtensions.GetMethodInfo((IPreviewBeatmapLevel previewBeatmapLevel) => UnwrapPlaylistSong(previewBeatmapLevel));

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && codes[i].operand.ToString() == "IPreviewBeatmapLevel _selectedPreviewBeatmapLevel")
                {
                    if (codes[i-1].opcode != OpCodes.Ldnull)
                    {
                        index = i;
                    }
                }
            }
            if (index != -1)
            {
                CodeInstruction newInstruction = new CodeInstruction(OpCodes.Callvirt, _unwrapPlaylistSong);
                codes.Insert(index, newInstruction);
            }
            return codes.AsEnumerable();
        }

        internal static IPreviewBeatmapLevel UnwrapPlaylistSong(IPreviewBeatmapLevel previewBeatmapLevel)
        {
            DidSelectLevelEvent.Invoke(previewBeatmapLevel);
            if (previewBeatmapLevel is IPlaylistSong)
            {
                return ((IPlaylistSong)previewBeatmapLevel).PreviewBeatmapLevel;
            }
            else
            {
                return previewBeatmapLevel;
            }
        }
    }
}
