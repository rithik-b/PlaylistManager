using HarmonyLib;
using HMUI;
using PlaylistManager.Types;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(ScrollView), nameof(ScrollView.UpdateVerticalScrollIndicator))]
    internal class ScrollView_UpdateVerticalScrollIndicator
    {
        private static bool Prefix(ref ScrollView __instance, float posY)
        {
            if (__instance is GridScrollView gridScrollView)
            {
                gridScrollView.UpdateVerticalScrollIndicator(posY);
                return false;
            }
            return true;
        }
    }
}
