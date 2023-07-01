using HarmonyLib;
using PlaylistManager.Utilities;
using Zenject;

namespace PlaylistManager.HarmonyPatches
{
    internal class MainSystemInitPatch
    {
        [HarmonyPatch(typeof(MainSystemInit), nameof(MainSystemInit.InstallBindings))]
        private static void Postfix(DiContainer container)
        {
            PlaylistLibUtils.coroutineStarter = container.Resolve<ICoroutineStarter>();
        }
    }
}
