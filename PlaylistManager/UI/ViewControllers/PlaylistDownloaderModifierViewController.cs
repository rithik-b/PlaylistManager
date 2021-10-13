using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using System;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    internal class PlaylistDownloaderModifierViewController : IInitializable, IDisposable
    {
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        public PlaylistDownloaderModifierViewController(GameplaySetupViewController gameplaySetupViewController, PlaylistDownloaderViewController playlistDownloaderViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.playlistDownloaderViewController = playlistDownloaderViewController;
        }

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Playlist Downloader", "PlaylistManager.UI.Views.Blank.bsml", this);
            gameplaySetupViewController.didActivateEvent += GameplaySetupViewController_didActivateEvent;
        }

        public void Dispose()
        {
            GameplaySetup.instance?.RemoveTab("Playlist Downloader");
            gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_didActivateEvent;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            playlistDownloaderViewController.__Init(gameplaySetupViewController.screen, gameplaySetupViewController, null);
            playlistDownloaderViewController.__Activate(false, false);
            playlistDownloaderViewController.transform.SetParent(rootTransform);
            playlistDownloaderViewController.transform.localPosition = Vector3.zero;
        }
    }
}
