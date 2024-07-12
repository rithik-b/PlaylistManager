using HarmonyLib;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.SetData))]
    internal class LevelCollectionViewController_SetData
    {
        private static void Postfix(LevelCollectionViewController __instance, BeatmapLevel[] beatmapLevels)
        {
            if (beatmapLevels == null || beatmapLevels.Length == 0)
            {
                __instance._levelCollectionTableView.gameObject.SetActive(true);
            }
        }
    }
}