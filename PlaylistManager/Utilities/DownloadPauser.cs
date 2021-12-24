using System;
using Zenject;

namespace PlaylistManager.Utilities
{
    internal class DownloadPauser : IInitializable, IDisposable
    {
        private readonly PlaylistDownloader playlistDownloader;

        public DownloadPauser(PlaylistDownloader playlistDownloader)
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
