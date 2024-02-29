using HMUI;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;
using SiraUtil.Affinity;
using UnityEngine.UI;

/*
 * Original Author: Auros
 * Taken from PlaylistCore
 */

namespace PlaylistManager.AffinityPatches
{
    internal class LevelCollectionCellSetDataPatch : IAffinity
    {
        private readonly HoverHintController hoverHintController;

        public LevelCollectionCellSetDataPatch(HoverHintController hoverHintController)
        {
            this.hoverHintController = hoverHintController;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.SetData))]
        private void Patch(AnnotatedBeatmapLevelCollectionCell __instance, ref BeatmapLevelPack beatmapLevelPack, ref Image ____coverImage)
        {
            if (PluginConfig.Instance.PlaylistHoverHints)
            {
                var hoverHint = __instance.GetComponent<HoverHint>();

                if (hoverHint == null)
                {
                    hoverHint = __instance.gameObject.AddComponent<HoverHint>();
                    Accessors.HoverHintControllerAccessor(ref hoverHint) = hoverHintController;
                }

                hoverHint.text = beatmapLevelPack.packName;
            }
        }
    }
}