using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using PlaylistManager.Utilities;
using System;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    internal class PlaylistDownloaderModifierViewController : IInitializable, IDisposable
    {
        private readonly PlaylistDownloader playlistDownloader;
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController;

        private bool _isVisible;

        private bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (IsVisible != value)
                {
                    _isVisible = value;
                    GameplaySetup.instance.SetTabVisibility("Playlist Downloader", value);
                }
            }
        }

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        public PlaylistDownloaderModifierViewController(PlaylistDownloader playlistDownloader,
            GameplaySetupViewController gameplaySetupViewController, PlaylistDownloaderViewController playlistDownloaderViewController)
        {
            this.playlistDownloader = playlistDownloader;
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.playlistDownloaderViewController = playlistDownloaderViewController;
        }

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Playlist Downloader", "PlaylistManager.UI.Views.Blank.bsml", this, MenuType.None);
            GameplaySetup.instance.TabsCreatedEvent += GameplaySetup_TabsCreatedEvent;
            playlistDownloader.QueueUpdatedEvent += PlaylistDownloader_QueueUpdatedEvent;
        }

        public void Dispose()
        {
            if (GameplaySetup.instance != null)
            {
                GameplaySetup.instance.RemoveTab("Playlist Downloader");
                GameplaySetup.instance.TabsCreatedEvent -= GameplaySetup_TabsCreatedEvent;
            }
            playlistDownloader.QueueUpdatedEvent -= PlaylistDownloader_QueueUpdatedEvent;
        }

        private void GameplaySetup_TabsCreatedEvent()
        {
            _isVisible = false;
            IsVisible = true;
            playlistDownloaderViewController.__Init(gameplaySetupViewController.screen, gameplaySetupViewController, null);
            playlistDownloaderViewController.__Activate(false, false);
            playlistDownloaderViewController.transform.SetParent(rootTransform);
            playlistDownloaderViewController.transform.localPosition = Vector3.zero;
            PlaylistDownloader_QueueUpdatedEvent();
        }

        private void PlaylistDownloader_QueueUpdatedEvent() => IsVisible = playlistDownloader.downloadQueue.Count > 0;
    }
}
