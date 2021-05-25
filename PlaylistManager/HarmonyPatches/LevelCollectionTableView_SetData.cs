using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController))]
    [HarmonyPatch("SetData", MethodType.Normal)]
    public class LevelCollectionTableView_SetData
    {
        internal static readonly MethodInfo _replaceWithPlaylistSongs = SymbolExtensions.GetMethodInfo((IBeatmapLevelCollection beatmapLevelCollection) => ReplaceWithPlaylistSongs(beatmapLevelCollection));

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = -1;
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand.ToString() == "IPreviewBeatmapLevel[] get_beatmapLevels()" )
                {
                    if (codes[i].opcode == OpCodes.Ldarg_1 && codes[i + 2].opcode == OpCodes.Stloc_0)
                    {
                        index = i + 1;
                    }
                }
            }
            if (index != -1)
            {
                codes[index] = new CodeInstruction(OpCodes.Callvirt, _replaceWithPlaylistSongs);
            }
            return codes.AsEnumerable();
        }

        internal static IPreviewBeatmapLevel[] ReplaceWithPlaylistSongs(IBeatmapLevelCollection beatmapLevelCollection)
        {
            if (beatmapLevelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist legacyPlaylist)
            {
                return legacyPlaylist.BeatmapLevels;
            }
            if (beatmapLevelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist blistPlaylist)
            {
                return blistPlaylist.BeatmapLevels;
            }
            return beatmapLevelCollection.beatmapLevels;
        }
    }
}
