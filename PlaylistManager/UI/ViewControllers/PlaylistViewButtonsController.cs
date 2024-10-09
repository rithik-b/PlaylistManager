﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.ComponentModel;
using IPA.Loader;
using PlaylistManager.Downloaders;
using SiraUtil.Zenject;
using Tweening;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    internal class PlaylistViewButtonsController : IInitializable, IDisposable, INotifyPropertyChanged, ILevelCategoryUpdater, IParentManagerUpdater
    {
        private readonly PopupModalsController popupModalsController;
        private readonly TweeningManager uwuTweenyManager;
        private readonly PlaylistSequentialDownloader playlistDownloader;
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController;
        private readonly PlaylistManagerFlowCoordinator playlistManagerFlowCoordinator;

        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelFilteringNavigationController levelFilteringNavigationController;
        private readonly SelectLevelCategoryViewController selectLevelCategoryViewController;
        private readonly IconSegmentedControl levelCategorySegmentedControl;
        private readonly PluginMetadata pluginMetadata;
        private readonly BSMLParser bsmlParser;

        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("download-button")]
        private readonly RectTransform downloadButtonTransform;

        private CurvedTextMeshPro downloadButtonText;

        private Color downloadButtonTextColor;

        [UIComponent("flow-button")]
        private readonly ButtonIconImage flowButton;

        [UIComponent("queue-modal")]
        private readonly ModalView queueModal;

        [UIComponent("queue-modal")]
        private readonly RectTransform queueModalTransform;

        private Vector3 queueModalPosition;

        public PlaylistViewButtonsController(PopupModalsController popupModalsController, TimeTweeningManager uwuTweenyManager, PlaylistSequentialDownloader playlistDownloader, PlaylistDownloaderViewController playlistDownloaderViewController,
            MainFlowCoordinator mainFlowCoordinator, PlaylistManagerFlowCoordinator playlistManagerFlowCoordinator, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController,
            LevelFilteringNavigationController levelFilteringNavigationController, SelectLevelCategoryViewController selectLevelCategoryViewController, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.popupModalsController = popupModalsController;
            this.uwuTweenyManager = uwuTweenyManager;
            this.playlistDownloader = playlistDownloader;
            this.playlistDownloaderViewController = playlistDownloaderViewController;

            this.mainFlowCoordinator = mainFlowCoordinator;
            this.playlistManagerFlowCoordinator = playlistManagerFlowCoordinator;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelFilteringNavigationController = levelFilteringNavigationController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            levelCategorySegmentedControl = selectLevelCategoryViewController._levelFilterCategoryIconSegmentedControl;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;
        }

        public void Initialize()
        {
            bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), annotatedBeatmapLevelCollectionsViewController.gameObject, this);
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
                var tween = new FloatTween(0.35f, 0.6f, val =>
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

            flowButton.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
            var icon = flowButton.Image as ImageView;
            icon._skew = 0.18f;
        }

        #region Create Playlist

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

            var playlist = PlaylistLibUtils.CreatePlaylistWithConfig(playlistName, parentManager ?? BeatSaberPlaylistsLib.PlaylistManager.DefaultManager);
            popupModalsController.ShowYesNoModal(rootTransform, $"Successfully created {playlist.Title}", () =>
            {
                // In case the category isn't already playlists which it shouldn't be
                levelCategorySegmentedControl.SelectCellWithNumber(1);
                selectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell(levelCategorySegmentedControl, 1);
                levelFilteringNavigationController.SelectAnnotatedBeatmapLevelCollection(playlist.PlaylistLevelPack);
            }, "Go to playlist", "Dismiss");
        }

        #endregion

        #region Download Queue

        [UIAction("queue-click")]
        private void ShowQueue()
        {
            queueModalTransform.localPosition = queueModalPosition;
            queueModal.Show(true, moveToCenter: false, finishedCallback: () =>
            {
                playlistDownloaderViewController.SetParent(queueModalTransform, new Vector3(0.75f, 0.75f, 1f));
            });
        }

        [UIValue("queue-interactable")]
        private bool QueueInteractable => PlaylistSequentialDownloader.downloadQueue.Count != 0;

        #endregion

        #region Settings

        [UIAction("flow-click")]
        private void ShowSettings()
        {
            playlistManagerFlowCoordinator.PresentFlowCoordinator(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf());
        }

        #endregion
    }
}
