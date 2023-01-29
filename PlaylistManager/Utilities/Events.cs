using System;

namespace PlaylistManager.Utilities
{
    public class Events
    {
        /// <summary>
        /// Raised when an <see cref="BeatSaberPlaylistsLib.Types.IPlaylistSong"/> is selected inside a playlist
        /// </summary>
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong>? playlistSongSelected;
        /// <summary>
        /// Raised when an <see cref="BeatSaberPlaylistsLib.Types.IPlaylist"/> is selected in the menu
        /// </summary>
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylist, BeatSaberPlaylistsLib.PlaylistManager>? playlistSelected;
        /// <summary>
        /// Raised when an <see cref="BeatSaberPlaylistsLib.Types.IPlaylistSong"/> is added to an <see cref="BeatSaberPlaylistsLib.Types.IPlaylist"/>
        /// </summary>
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong, BeatSaberPlaylistsLib.Types.IPlaylist>? playlistSongAdded;
        /// <summary>
        /// Raised when an <see cref="BeatSaberPlaylistsLib.Types.IPlaylistSong"/> is removed from an <see cref="BeatSaberPlaylistsLib.Types.IPlaylist"/>
        /// </summary>
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylistSong, BeatSaberPlaylistsLib.Types.IPlaylist>? playlistSongRemoved;
        /// <summary>
        /// Raised when an <see cref="BeatSaberPlaylistsLib.Types.IPlaylist"> is renamed
        /// </summary>
        public static event Action<BeatSaberPlaylistsLib.Types.IPlaylist, BeatSaberPlaylistsLib.PlaylistManager>? playlistRenamed;

        internal static void RaisePlaylistSongSelected(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong) => playlistSongSelected?.Invoke(playlistSong);
        internal static void RaisePlaylistSelected(BeatSaberPlaylistsLib.Types.IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager) => playlistSelected?.Invoke(playlist, parentManager);
        internal static void RaisePlaylistSongAdded(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong, BeatSaberPlaylistsLib.Types.IPlaylist playlist) => playlistSongAdded?.Invoke(playlistSong, playlist);
        internal static void RaisePlaylistSongRemoved(BeatSaberPlaylistsLib.Types.IPlaylistSong playlistSong, BeatSaberPlaylistsLib.Types.IPlaylist playlist) => playlistSongRemoved?.Invoke(playlistSong, playlist);
        internal static void RaisePlaylistRenamed(BeatSaberPlaylistsLib.Types.IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager) => playlistRenamed?.Invoke(playlist, parentManager);
    }
}
