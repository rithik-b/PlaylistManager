using System;

namespace PlaylistManager.Utilities
{
    public class Events
    {
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong> playlistSongSelected;
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylist, BeatSaberPlaylistsLib.PlaylistManager> playlistSelected;

        internal static void RaisePlaylistSongSelected(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong) => playlistSongSelected?.Invoke(playlistSong);
        internal static void RaisePlaylistSelected(BeatSaberPlaylistsLib.Types.IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager) => playlistSelected?.Invoke(playlist, parentManager);
    }
}
