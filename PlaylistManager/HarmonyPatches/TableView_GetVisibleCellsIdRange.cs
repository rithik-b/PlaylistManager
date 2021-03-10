using HarmonyLib;
using HMUI;
using PlaylistManager.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(TableView))]
    [HarmonyPatch("GetVisibleCellsIdRange", MethodType.Normal)]
    public class TableView_GetVisibleCellsIdRange
    {
        private static TableView.TableType _tableType;
        internal static readonly MethodInfo _reversePosition = SymbolExtensions.GetMethodInfo((ScrollView scrollView) => ReversePosition(scrollView));

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = -1;

            for (int i = 3; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i-1].opcode == OpCodes.Ldfld && codes[i - 1].operand.ToString() == "HMUI.ScrollView _scrollView" && codes[i-2].opcode == OpCodes.Ldarg_0 && codes[i-3].opcode == OpCodes.Stloc_1)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                codes[index] = new CodeInstruction(OpCodes.Callvirt, _reversePosition);
            }

            return codes.AsEnumerable();
        }

        internal static void Prefix(ref TableView.TableType ____tableType)
        {
            _tableType = ____tableType;
        }

        internal static float ReversePosition(ScrollView scrollView) 
        {
            if (_tableType == TableView.TableType.Horizontal)
            {
                return -scrollView.position;
            }
            return scrollView.position;
        }
    }
}