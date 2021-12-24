using System;

namespace PlaylistManager.Utilities
{
    public class Events
    {
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong> playlistSongSelected;
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylist, BeatSaberPlaylistsLib.PlaylistManager> playlistSelected;
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong, BeatSaberPlaylistsLib.Types.IPlaylist> playlistSongAdded;
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong, BeatSaberPlaylistsLib.Types.IPlaylist> playlistSongRemoved;
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylist, BeatSaberPlaylistsLib.PlaylistManager> playlistRenamed;

        internal static void RaisePlaylistSongSelected(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong) => playlistSongSelected?.Invoke(playlistSong);
        internal static void RaisePlaylistSelected(BeatSaberPlaylistsLib.Types.IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager) => playlistSelected?.Invoke(playlist, parentManager);
        internal static void RaisePlaylistSongAdded(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong, BeatSaberPlaylistsLib.Types.IPlaylist playlist) => playlistSongAdded?.Invoke(playlistSong, playlist);
        internal static void RaisePlaylistSongRemoved(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong, BeatSaberPlaylistsLib.Types.IPlaylist playlist) => playlistSongRemoved?.Invoke(playlistSong, playlist);
        internal static void RaisePlaylistRenamed(BeatSaberPlaylistsLib.Types.IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager) => playlistRenamed?.Invoke(playlist, parentManager);
    }
}
