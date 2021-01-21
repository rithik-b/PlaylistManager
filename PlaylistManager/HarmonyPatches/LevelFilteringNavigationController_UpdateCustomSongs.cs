using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController), "UpdateSecondChildControllerContent", 
        new Type[] {
        typeof(SelectLevelCategoryViewController.LevelCategory)})]
    public class LevelFilteringNavigationController_UpdateCustomSongs
    {
        internal static event Action CustomSongsUpdatedEvent;
        internal static void Postfix()
        {
            CustomSongsUpdatedEvent?.Invoke();
        }
    }
}
