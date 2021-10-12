using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using PlaylistManager.Utilities;
using System;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\PlaylistDownloaderView.bsml")]
    [ViewDefinition("PlaylistManager.UI.Views.PlaylistDownloaderView.bsml")]
    internal class PlaylistDownloaderViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private PlaylistDownloader playlistDownloader;

        private FloatingScreen floatingScreen;


        [Inject]
        public void Construct(PlaylistDownloader playlistDownloader)
        {
            this.playlistDownloader = playlistDownloader;
        }

        public void Initialize()
        {
            floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(120f, 120f), true, Vector3.zero, Quaternion.identity);
            floatingScreen.HighlightHandle = true;
            floatingScreen.SetRootViewController(this, AnimationType.In);
        }

        public void Dispose()
        {
            if (floatingScreen != null && floatingScreen.gameObject != null)
            {
                Destroy(floatingScreen.gameObject);
            }
        }
    }
}
