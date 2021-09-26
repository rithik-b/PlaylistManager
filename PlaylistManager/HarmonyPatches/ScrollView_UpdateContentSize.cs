using HarmonyLib;
using HMUI;
using PlaylistManager.Types;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(ScrollView), nameof(ScrollView.UpdateContentSize))]
    internal class ScrollView_UpdateContentSize
    {
        private static bool Prefix(ref ScrollView __instance)
        {
            if (__instance is GridScrollView gridScrollView)
            {
                gridScrollView.UpdateContentSize();
                return false;
            }
            return true;
        }
    }
}