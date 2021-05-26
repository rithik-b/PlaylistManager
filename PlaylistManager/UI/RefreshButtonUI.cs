using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;
using SongCore;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    public class RefreshButtonUI : IInitializable, IDisposable
    {
        public MenuButton _refreshButton;
        internal ProgressBar _progressBar;
        const int MESSAGE_TIME = 5;

        public void Initialize()
        {
            _refreshButton = new MenuButton("Refresh Playlists", "Refresh Songs & Playlists", RefreshButtonPressed, true);
            MenuButtons.instance.RegisterButton(_refreshButton);
        }

        private void SongsLoaded(Loader _, System.Collections.Concurrent.ConcurrentDictionary<string, CustomPreviewBeatmapLevel> songs)
        {
            PlaylistLibUtils.playlistManager.RefreshPlaylists(!PluginConfig.Instance.FoldersDisabled);

            if (_progressBar == null)
            {
                _progressBar = ProgressBar.Create();
            }
            int numPlaylists = PlaylistLibUtils.playlistManager.GetAllPlaylists(!PluginConfig.Instance.FoldersDisabled).Length;

            _progressBar.enabled = true;
            _progressBar.ShowMessage(string.Format("\n{0} playlists loaded.", numPlaylists), MESSAGE_TIME);
            Loader.SongsLoadedEvent -= SongsLoaded;
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
            Loader.SongsLoadedEvent += SongsLoaded;
        }
    }
}