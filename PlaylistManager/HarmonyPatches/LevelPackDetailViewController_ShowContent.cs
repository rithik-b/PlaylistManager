using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using PlaylistManager.Configuration;
using UnityEngine;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelPackDetailViewController), nameof(LevelPackDetailViewController.ShowContent))]
    internal class LevelPackDetailViewController_ShowContent
    {
        private static bool Prefix(LevelPackDetailViewController __instance, LevelPackDetailViewController.ContentType contentType)
        {
            if (contentType == LevelPackDetailViewController.ContentType.NonBuyable && __instance._pack is PlaylistLevelPack)
            {
                if (PluginConfig.Instance.BlurredArt)
                {
                    __instance._packImage.sprite = __instance._blurredPackArtwork;
                }
                __instance._packImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                __instance._loadingControl.Hide();
                __instance._detailWrapper.SetActive(true);
                return false;
            }
            return true;
        }
    }
}
