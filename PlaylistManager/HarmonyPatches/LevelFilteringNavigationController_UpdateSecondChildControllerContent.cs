using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController), "UpdateSecondChildControllerContent", 
        new Type[] {
        typeof(SelectLevelCategoryViewController.LevelCategory)})]
    public class LevelFilteringNavigationController_UpdateSecondChildControllerContent
    {
        internal static event Action SecondChildControllerUpdatedEvent;
        internal static void Postfix()
        {
            SecondChildControllerUpdatedEvent?.Invoke();
        }
    }
}
