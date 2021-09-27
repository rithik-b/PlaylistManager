using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.AnimateOpen))]
    internal class AnnotatedBeatmapLevelCollectionsGridViewAnimator_AnimateOpen
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = -1;
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_3  && codes[i + 1].opcode == OpCodes.Call)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                codes[index] = new CodeInstruction(OpCodes.Ldc_R4, 135f);
            }
            return codes.AsEnumerable();
        }

    }
}
