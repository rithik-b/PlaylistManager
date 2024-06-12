using HarmonyLib;
using System;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.UpdateSecondChildControllerContent))]
    public class LevelFilteringNavigationController_UpdateSecondChildControllerContent
    {
        internal static event Action SecondChildControllerUpdatedEvent;
        internal static void Postfix()
        {
            SecondChildControllerUpdatedEvent?.Invoke();
        }
    }
}
