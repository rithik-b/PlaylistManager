using BeatSaberPlaylistsLib.Types;
using HarmonyLib;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionTableView))]
    [HarmonyPatch("SetData", MethodType.Normal)]
    public class LevelCollectionTableView_SetData
    {
        /*
         * Since SongBrowser (or other filter mods) can set the TableView directly without the need of using Playlist or other collection types
         * This patch will unwrap them
         */

        internal static void Prefix(ref IPreviewBeatmapLevel[] previewBeatmapLevels)
        {
            // Safe to assume if first song is IPlaylistSong, there will be more
            if (previewBeatmapLevels.Length != 0 && previewBeatmapLevels[0] is IPlaylistSong)
            {
                // Clone so the filtered collection we originally got will still have the PlaylistLib types
                previewBeatmapLevels = (IPreviewBeatmapLevel[])previewBeatmapLevels.Clone();
                for (int i = 0; i < previewBeatmapLevels.Length; i++)
                {
                    // If statement anyway for safety
                    if (previewBeatmapLevels[i] is IPlaylistSong playlistSong)
                    {
                        previewBeatmapLevels[i] = playlistSong.PreviewBeatmapLevel;
                    }
                }
            }
        }
    }
}
