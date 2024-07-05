using System;
using System.Collections.Generic;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;
using HMUI;
using PlaylistManager.Configuration;
using PlaylistManager.UI;
using SiraUtil.Affinity;

/*
 * Original Author: Auros
 * Taken from PlaylistCore
 */

namespace PlaylistManager.AffinityPatches
{
    internal class LevelCollectionCellSetDataPatch : IAffinity
    {
        private readonly Dictionary<IPlaylist, AnnotatedBeatmapLevelCollectionCell> eventTable = new();
        private readonly HoverHintController hoverHintController;
        private readonly PlaylistUpdater playlistUpdater;

        public LevelCollectionCellSetDataPatch(HoverHintController hoverHintController, PlaylistUpdater playlistUpdater)
        {
            this.hoverHintController = hoverHintController;
            this.playlistUpdater = playlistUpdater;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.SetData))]
        private void Patch(AnnotatedBeatmapLevelCollectionCell __instance, ref BeatmapLevelPack beatmapLevelPack)
        {
            if (beatmapLevelPack is PlaylistLevelPack playlistLevelPack)
            {
                var playlist = playlistLevelPack.playlist;
                eventTable.Remove(playlist);
                eventTable.Add(playlist, __instance);
                playlist.SpriteLoaded -= OnSpriteLoaded;
                playlist.SpriteLoaded += OnSpriteLoaded;
            }

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

        private void OnSpriteLoaded(object sender, EventArgs e)
        {
            // TODO: Figure out why this doesn't seem to happen.
            if (sender is not IPlaylist playlist)
            {
                return;
            }

            playlist.SpriteLoaded -= OnSpriteLoaded;

            if (!eventTable.TryGetValue(playlist, out var tableCell) || tableCell == null || tableCell._beatmapLevelPack is not PlaylistLevelPack)
            {
                return;
            }

            tableCell._coverImage.sprite = playlist.SmallSprite;
            // TODO: Figure out why this needs to be done here and in UpdatePlaylist when switching covers. Worth noting that this event is invoked twice as well.
            playlistUpdater.RefreshAnnotatedBeatmapCollection(tableCell._beatmapLevelPack);
        }
    }
}