using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using PlaylistManager.Utilities;
using SongCore;
using SongCore.UI;
using System;
using System.Threading.Tasks;
using Zenject;

namespace PlaylistManager.UI
{
    public class RefreshButtonUI : IInitializable, IDisposable
    {
        private MenuButton refreshButton;
        private ProgressBar progressBar;
        private const int kMessageTime = 5;

        public void Initialize()
        {
            refreshButton = new MenuButton("Refresh Playlists", "Refresh Songs & Playlists", RefreshButtonPressed);
            MenuButtons.instance.RegisterButton(refreshButton);
            Loader.SongsLoadedEvent += SongsLoaded;
        }

        private async void SongsLoaded(Loader _, System.Collections.Concurrent.ConcurrentDictionary<string, CustomPreviewBeatmapLevel> songs)
        {
            if (progressBar == null)
            {
                progressBar = ProgressBar.Create();
            }

            var numPlaylists = await Task.Run(() => PlaylistLibUtils.TryGetAllPlaylists().Length).ConfigureAwait(false);

            progressBar.enabled = true;
            progressBar.ShowMessage($"\n{numPlaylists} playlists loaded.", kMessageTime);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(progressBar);
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