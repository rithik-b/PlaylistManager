using HarmonyLib;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PlaylistLoaderLite.HarmonyPatches
{
    [HarmonyPatch(typeof(TableViewScroller), "HandleJoystickWasNotCenteredThisFrame",
        new Type[] {
        typeof(Vector2)})]
    public class ScrollingFix
    {
        private static Vector2 _deltaPos;
        private static TableViewScroller _instance;
        internal static readonly MethodInfo _swappedComparison = SymbolExtensions.GetMethodInfo(() => SwappedComparison());

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int startIndex = -1;
            bool foundMiddle = false;
            int endIndex = -1;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_1)
                {
                    if (startIndex == -1)
                        startIndex = i + 1;
                    else if (foundMiddle == false)
                        foundMiddle = true;
                    else
                        endIndex = i + 1;
                }
            }

            if (startIndex != -1 && endIndex != -1)
            {
                codes.RemoveRange(startIndex, endIndex - startIndex);
                CodeInstruction newInstruction = new CodeInstruction(OpCodes.Callvirt, _swappedComparison);
                codes.Insert(startIndex, newInstruction);
                codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Stloc_1));
            }

            return codes.AsEnumerable();
        }

        internal static void Prefix(ref Vector2 deltaPos, ref TableViewScroller __instance)
        {
            _deltaPos = deltaPos;
            _instance = __instance;
        }

        internal static float SwappedComparison() 
        {
            float num2 = _instance.position - _deltaPos.x * Time.deltaTime * 60f;
            float fixedScrollableSize = - _instance.scrollableSize;

            if (num2 < fixedScrollableSize)
                num2 = fixedScrollableSize;

            if (num2 > 0.0f)
                num2 = 0.0f;

            return num2;
        }
    }
}