using HarmonyLib;
using HMUI;
using PlaylistManager.Types;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(ScrollView), nameof(ScrollView.RefreshButtons))]
    internal class ScrollView_RefreshButtons
    {
        private static bool Prefix(ScrollView __instance)
        {
            if (__instance is GridScrollView gridScrollView)
            {
                gridScrollView.RefreshButtons();
                return false;
            }
            return true;
        }
    }
}
