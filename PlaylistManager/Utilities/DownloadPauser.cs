using System;
using PlaylistManager.Downloaders;
using Zenject;

namespace PlaylistManager.Utilities
{
    internal class DownloadPauser : IInitializable, IDisposable
    {
        private readonly PlaylistSequentialDownloader playlistDownloader;

        public DownloadPauser(PlaylistSequentialDownloader playlistDownloader)
        {
            this.playlistDownloader = playlistDownloader;
        }

        public void Initialize()
        {
            playlistDownloader.PauseDownload();
        }

        public void Dispose()
        {
            playlistDownloader.ResumeDownload();
        }
    }
}
