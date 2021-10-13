using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(SongCore.Loader), "MenuLoaded")]
    internal class SongCore_MenuLoaded
    {
        public static event Action MenuLoadedEvent;
        private static void Postfix()
        {
            MenuLoadedEvent?.Invoke();
        }
    }
}
