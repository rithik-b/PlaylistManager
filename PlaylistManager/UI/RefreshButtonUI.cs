using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using PlaylistManager.Utilities;
using SongCore;
using System;
using System.Threading.Tasks;
using Zenject;

namespace PlaylistManager.UI
{
    public class RefreshButtonUI : IInitializable, IDisposable
    {
        public MenuButton _refreshButton;
        internal ProgressBar _progressBar;
        const int MESSAGE_TIME = 5;
        const string REQUEST_SOURCE = "PlaylistManager (Plugin)";

        public void Initialize()
        {
            _refreshButton = new MenuButton("Refresh Playlists", "Refresh Songs & Playlists", RefreshButtonPressed, true);
            MenuButtons.instance.RegisterButton(_refreshButton);
            Loader.SongsLoadedEvent += SongsLoaded;
        }

        private void SongsLoaded(Loader _, System.Collections.Concurrent.ConcurrentDictionary<string, CustomPreviewBeatmapLevel> songs)
        {
            if (_progressBar == null)
            {
                _progressBar = ProgressBar.Create();
            }

            PlaylistLibUtils.playlistManager.RequestRefresh(REQUEST_SOURCE); // For now does nothing, need to ask the author of manager to implement actual refresh
            int numPlaylists = PlaylistLibUtils.playlistManager.GetAllPlaylists(true).Length;

            _progressBar.enabled = true;
            _progressBar.ShowMessage(string.Format("\n{0} playlists loaded.", numPlaylists), MESSAGE_TIME);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_progressBar);
            if (BSMLParser.IsSingletonAvailable && MenuButtons.IsSingletonAvailable)
                MenuButtons.instance.UnregisterButton(_refreshButton);
            Loader.SongsLoadedEvent -= SongsLoaded;
        }

        internal void RefreshButtonPressed()
        {
            if (!Loader.AreSongsLoading)
                Loader.Instance.RefreshSongs(fullRefresh: false);
        }
    }
}