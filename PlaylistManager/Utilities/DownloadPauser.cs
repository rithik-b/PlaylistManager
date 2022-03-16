using System;
using Zenject;

namespace PlaylistManager.Downloaders
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
