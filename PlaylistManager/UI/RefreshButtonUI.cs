using BeatSaberMarkupLanguage.MenuButtons;
using UnityEngine;
using SongCore;
using System.Collections;
using PlaylistManager.Utilities;

namespace PlaylistManager.UI
{
    public class RefreshButtonUI : PersistentSingleton<RefreshButtonUI>
    {
        public MenuButton _refreshButton;
        internal static ProgressBar _progressBar;
        const int MESSAGE_TIME = 5;
        const string REQUEST_SOURCE = "PlaylistManager (Plugin)";
        internal void Register()
        {
            _refreshButton = new MenuButton("Refresh Playlists", "Refresh Songs & Playlists", RefreshButtonPressed, true);
            MenuButtons.instance.RegisterButton(_refreshButton);
            LaunchLoadPlaylists();
        }

        internal void Unregister()
        {
            MenuButtons.instance.UnregisterButton(_refreshButton);
        }

        internal void LaunchLoadPlaylists()
        {
            StartCoroutine(LaunchLoadPlaylistsFlow());
        }

        internal void RefreshButtonPressed()
        {
            StartCoroutine(RefreshButtonFlow());
        }

        internal IEnumerator LaunchLoadPlaylistsFlow()
        {
            // Wait for SongCore plugin to load
            yield return new WaitUntil(() => Loader.Instance != null);
            _progressBar = ProgressBar.Create();
            StartCoroutine(RefreshButtonFlow());
        }

        internal IEnumerator RefreshButtonFlow()
        {
            if (!Loader.AreSongsLoading)
                Loader.Instance.RefreshSongs(fullRefresh: false);

            yield return new WaitUntil(() => Loader.AreSongsLoaded == true);
            PlaylistLibUtils.playlistManager.RequestRefresh(REQUEST_SOURCE);
            int numPlaylists = PlaylistLibUtils.playlistManager.GetAllPlaylists(true).Length;

            _progressBar.enabled = true;
            _progressBar.ShowMessage(string.Format("\n{0} playlists loaded.", numPlaylists), MESSAGE_TIME);
        }
    }
}