using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), nameof(AnnotatedBeatmapLevelCollectionsViewController.SetData))]
    internal class AnnotatedBeatmapLevelCollectionsViewController_SetData
    {
        private static readonly Dictionary<IPlaylist, AnnotatedBeatmapLevelCollectionsViewController> eventTable = new();

        private static void Postfix(AnnotatedBeatmapLevelCollectionsViewController __instance, IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections)
        {
            foreach (PlaylistLevelPack playlistLevelPack in annotatedBeatmapLevelCollections.OfType<PlaylistLevelPack>())
            {
                IPlaylist playlist = playlistLevelPack.playlist;

                if (playlist.SmallSpriteWasLoaded)
                {
                    continue;
                }

                eventTable.Remove(playlist);
                eventTable.Add(playlist, __instance);
                playlist.SpriteLoaded -= OnSpriteLoaded;
                playlist.SpriteLoaded += OnSpriteLoaded;
            }
        }

        private static void OnSpriteLoaded(object sender, EventArgs e)
        {
            if (sender is not IPlaylist playlist)
            {
                return;
            }

            playlist.SpriteLoaded -= OnSpriteLoaded;

            if (!eventTable.TryGetValue(playlist, out var vc) || vc == null)
            {
                return;
            }

            var annotatedBeatmapLevelCollections = CloneAndOverwriteEntry(vc._annotatedBeatmapLevelCollections, playlist.PlaylistLevelPack);
            vc._annotatedBeatmapLevelCollections = annotatedBeatmapLevelCollections;
            vc._annotatedBeatmapLevelCollectionsGridView.SetData(annotatedBeatmapLevelCollections);
        }

        private static IReadOnlyList<BeatmapLevelPack> CloneAndOverwriteEntry(IReadOnlyList<BeatmapLevelPack> original, BeatmapLevelPack item)
        {
            BeatmapLevelPack[] beatmapLevelPackCollection = new BeatmapLevelPack[original.Count];

            for (int i = 0; i < beatmapLevelPackCollection.Length; ++i)
            {
                if (original[i].packID == item.packID)
                {
                    beatmapLevelPackCollection[i] = item;
                }
                else
                {
                    beatmapLevelPackCollection[i] = original[i];
                }
            }

            return beatmapLevelPackCollection;
        }
    }
}
