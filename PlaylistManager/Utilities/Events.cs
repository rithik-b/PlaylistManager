using BeatSaberPlaylistsLib.Types;
using System;

namespace PlaylistManager.Utilities
{
    class Events
    {
        public static event Action<IPlaylistSong> playlistSongSelected;
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylist> playlistSelected;

        internal static void RaisePlaylistSongSelected(IPlaylistSong playlistSong) => playlistSongSelected?.Invoke(playlistSong);
        internal static void RaisePlaylistSelected(BeatSaberPlaylistsLib.Types.IPlaylist playlist) => playlistSelected?.Invoke(playlist);
    }
}
