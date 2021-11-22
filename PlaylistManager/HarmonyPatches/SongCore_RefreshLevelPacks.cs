using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(SongCore.Loader), nameof(SongCore.Loader.RefreshLevelPacks))]
    internal class SongCore_RefreshLevelPacks
    {
        public static event Action PacksToBeRefreshedEvent;
        private static void Prefix()
        {
            PacksToBeRefreshedEvent?.Invoke();
        }
    }
}
