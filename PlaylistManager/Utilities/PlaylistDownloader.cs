using System;
using PlaylistManager.Downloaders;
using PlaylistManager.Utilities;
using PlaylistManager.Types;

namespace PlaylistManager.Utilities
{
    /// <summary>
    /// The Playlist Downloader in PlaylistManager! Use it to queue playlists in the mod.
    /// </summary>
    public class PlaylistDownloader
    {
        private readonly PlaylistSequentialDownloader playlistSequentialDownloader;
        internal static event Action<DownloadQueueEntry>? PlaylistQueuedEvent;
        
        internal PlaylistDownloader(PlaylistSequentialDownloader playlistSequentialDownloader)
        {
            this.playlistSequentialDownloader = playlistSequentialDownloader;
        }

        /// <summary>
        /// Queue a playlist to download
        /// </summary>
        /// <param name="downloadQueueEntry"></param>
        public static void Queue(DownloadQueueEntry downloadQueueEntry)
        {
            PlaylistSequentialDownloader.downloadQueue.Add(downloadQueueEntry);
            PlaylistQueuedEvent?.Invoke(downloadQueueEntry);
        }
        
        /// <summary>
        /// Queue a playlist to download
        /// </summary>
        /// <param name="downloadQueueEntry"></param>
        public void QueuePlaylist(DownloadQueueEntry downloadQueueEntry) => playlistSequentialDownloader.QueuePlaylist(downloadQueueEntry);
    }
}