using HarmonyLib;
using HMUI;
using PlaylistManager.Types;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(ScrollView), nameof(ScrollView.SetDestinationPos))]
    internal class ScrollView_SetDestinationPos
    {
        private static bool Prefix(float value, ref ScrollView __instance)
        {
            if (__instance is GridScrollView gridScrollView)
            {
                gridScrollView.SetDestinationPos(value);
                return false;
            }
            return true;
        }
    }
}
