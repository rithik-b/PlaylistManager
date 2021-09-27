using HarmonyLib;
using HMUI;
using PlaylistManager.Configuration;
using UnityEngine;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelPackDetailViewController), nameof(LevelPackDetailViewController.ShowContent))]
    internal class LevelPackDetailViewController_ShowContent
    {
        private static bool Prefix(LevelPackDetailViewController.ContentType contentType, IBeatmapLevelPack ____pack, ImageView ____packImage, Sprite ____blurredPackArtwork, GameObject ____detailWrapper, LoadingControl ____loadingControl)
        {
            if (contentType == LevelPackDetailViewController.ContentType.Owned && ____pack is BeatSaberPlaylistsLib.Types.IPlaylist)
            {
                if (PluginConfig.Instance.BlurredArt)
                {
                    ____packImage.sprite = ____blurredPackArtwork;
                }
                ____packImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                ____loadingControl.Hide();
                ____detailWrapper.SetActive(true);
                return false;
            }
            return true;
        }
    }
}
