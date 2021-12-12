using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.ComponentModel;
using System.Reflection;
using Tweening;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    internal class PlaylistViewButtonsController : IInitializable, IDisposable, INotifyPropertyChanged, ILevelCategoryUpdater, IParentManagerUpdater
    {
        private readonly PopupModalsController popupModalsController;
        private readonly TweeningManager uwuTweenyManager;
        private readonly PlaylistDownloader playlistDownloader;
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController;
        private readonly SettingsViewController settingsViewController;
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;

        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("download-button")]
        private readonly RectTransform downloadButtonTransform;

        private CurvedTextMeshPro downloadButtonText;

        private Color downloadButtonTextColor;

        [UIComponent("queue-modal")]
        private readonly ModalView queueModal;

        [UIComponent("queue-modal")]
        private readonly RectTransform queueModalTransform;

        private Vector3 queueModalPosition;

        public PlaylistViewButtonsController(PopupModalsController popupModalsController, TimeTweeningManager uwuTweenyManager, PlaylistDownloader playlistDownloader, PlaylistDownloaderViewController playlistDownloaderViewController,
            MainFlowCoordinator mainFlowCoordinator, SettingsViewController settingsViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController)
        {
            this.popupModalsController = popupModalsController;
            this.uwuTweenyManager = uwuTweenyManager;
            this.playlistDownloader = playlistDownloader;
            this.playlistDownloaderViewController = playlistDownloaderViewController;
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.settingsViewController = settingsViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), annotatedBeatmapLevelCollectionsViewController.gameObject, this);
            playlistDownloader.QueueUpdatedEvent += DownloadQueueUpdated;
            playlistDownloader.PopupEvent += TweenButton;
        }

        public void Dispose()
        {
            playlistDownloader.QueueUpdatedEvent -= DownloadQueueUpdated;
            playlistDownloader.PopupEvent -= TweenButton;
        }

        private void DownloadQueueUpdated()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QueueInteractable)));
        }

        private void TweenButton()
        {
            uwuTweenyManager.KillAllTweens(downloadButtonText);
            if (playlistDownloader.PendingPopup != null)
            {
                FloatTween tween = new FloatTween(0.35f, 0.6f, val =>
                {
                    downloadButtonText.color = new Color(val, val, val);
                }, 0.75f, EaseType.InOutBack);
                uwuTweenyManager.AddTween(tween, downloadButtonText);
                tween.onCompleted = delegate () { TweenButton(); };
            }
            else
            {
                downloadButtonText.color = downloadButtonTextColor;
            }
        }

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory, bool viewControllerActivated)
        {
            if (rootTransform != null)
            {
                if (levelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
                {
                    rootTransform.gameObject.SetActive(true);
                }
                else
                {
                    rootTransform.gameObject.SetActive(false);
                }
            }
        }

        public void ParentManagerUpdated(BeatSaberPlaylistsLib.PlaylistManager parentManager) => this.parentManager = parentManager;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            queueModalPosition = queueModalTransform.localPosition;
            downloadButtonText = downloadButtonTransform.GetComponentInChildren<CurvedTextMeshPro>();
            downloadButtonTextColor = downloadButtonText.color;
        }

        [UIAction("create-click")]
        private void CreateClicked()
        {
            popupModalsController.ShowKeyboard(rootTransform, CreatePlaylist);
        }

        private void CreatePlaylist(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return;
            }

            BeatSaberPlaylistsLib.Types.IPlaylist playlist = PlaylistLibUtils.CreatePlaylistWithConfig(playlistName, parentManager ?? BeatSaberPlaylistsLib.PlaylistManager.DefaultManager);
            popupModalsController.ShowOkModal(rootTransform, $"Successfully created {playlist.collectionName}", null);
        }

        [UIAction("queue-click")]
        private void ShowQueue()
        {
            queueModalTransform.localPosition = queueModalPosition;
            queueModal.Show(true, moveToCenter: false, finishedCallback: () =>
            {
                playlistDownloaderViewController.SetParent(queueModalTransform, new Vector3(0.75f, 0.75f, 1f));
            });
        }

        [UIAction("settings-click")]
        private void ShowSettings()
        {
            mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf().InvokeMethod<object, FlowCoordinator>("PresentViewController", new object[] { settingsViewController, null, ViewController.AnimationDirection.Vertical, false });
        }

        [UIValue("queue-interactable")]
        private bool QueueInteractable => PlaylistDownloader.downloadQueue.Count != 0;
    }
}
