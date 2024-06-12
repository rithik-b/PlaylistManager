using BeatSaberMarkupLanguage.MenuButtons;
using PlaylistManager.Utilities;
using SongCore;
using SongCore.UI;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    public class RefreshButtonUI : IInitializable, IDisposable
    {
        private readonly ProgressBar _progressBar;

        private MenuButton refreshButton;

        private RefreshButtonUI(ProgressBar progressBar)
        {
            _progressBar = progressBar;
        }

        public void Initialize()
        {
            refreshButton = new MenuButton("Refresh Playlists", "Refresh Songs & Playlists", RefreshButtonPressed);
            MenuButtons.instance.RegisterButton(refreshButton);
            Loader.SongsLoadedEvent += SongsLoaded;
        }

        private void SongsLoaded(Loader _, System.Collections.Concurrent.ConcurrentDictionary<string, BeatmapLevel> songs)
        {
            PlaylistLibUtils.playlistManager.RefreshPlaylists(true);
            var numPlaylists = PlaylistLibUtils.playlistManager.GetPlaylistCount(true);
            _progressBar.AppendText($"\n{numPlaylists} playlists loaded");
        }

        public void Dispose()
        {
            MenuButtons.instance.UnregisterButton(refreshButton);
            Loader.SongsLoadedEvent -= SongsLoaded;
        }

        private void RefreshButtonPressed()
        {
            if (!Loader.AreSongsLoading)
            {
                Loader.Instance.RefreshSongs(fullRefresh: false);
            }
        }
    }
}