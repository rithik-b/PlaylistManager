﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using IPA.Loader;
using PlaylistManager.Downloaders;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistDownloaderViewController : MonoBehaviour, IInitializable, IDisposable
    {
        private PlaylistSequentialDownloader playlistDownloader;
        private PopupModalsController popupModalsController;
        private PluginMetadata pluginMetadata;
        private BSMLParser bsmlParser;
        private bool parsed;
        private bool refreshRequested;

        [UIComponent("download-list")]
        private readonly CustomCellListTableData customListTableData;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [Inject]
        internal void Construct(PlaylistSequentialDownloader playlistDownloader, PopupModalsController popupModalsController, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.playlistDownloader = playlistDownloader;
            this.popupModalsController = popupModalsController;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;
        }

        public void SetParent(Transform parent, Vector3? scale = null)
        {
            if (!parsed)
            {
                bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.PlaylistDownloaderView.bsml"), parent.gameObject, this);
            }
            rootTransform.SetParent(parent, false);
            rootTransform.localScale = scale ?? Vector3.one;
            OnPopupRequested();
        }

        public void OnDisable()
        {
            if (playlistDownloader.PendingPopup != null)
            {
                popupModalsController.HideYesNoModal();
            }
        }

        public void Initialize()
        {
            playlistDownloader.PopupEvent += OnPopupRequested;
            playlistDownloader.QueueUpdatedEvent += UpdateQueue;
            SceneManager.activeSceneChanged += OnMenuLoaded;
        }

        public void Dispose()
        {
            playlistDownloader.PopupEvent -= OnPopupRequested;
            playlistDownloader.QueueUpdatedEvent -= UpdateQueue;
            SceneManager.activeSceneChanged -= OnMenuLoaded;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            parsed = true;
            transform.SetParent(rootTransform);
            customListTableData.Data = PlaylistSequentialDownloader.downloadQueue;
            customListTableData.TableView.ReloadDataKeepingPosition();
        }

        private void OnPopupRequested()
        {
            if (playlistDownloader.PendingPopup != null && parsed)
            {
                IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(DontMessWithGameObjectsOffMainThread);
            }
        }

        private void DontMessWithGameObjectsOffMainThread()
        {
            playlistDownloader.PendingPopup.parent = rootTransform;
            playlistDownloader.PendingPopup.animateParentCanvas = !rootTransform.GetComponentInParent<ModalView>();
            if (rootTransform.gameObject.activeInHierarchy)
            {
                popupModalsController.ShowModal(playlistDownloader.PendingPopup);
            }
        }

        private void UpdateQueue()
        {
            if (customListTableData != null)
            {
                customListTableData.TableView.ReloadDataKeepingPosition();
            }

            if (PlaylistSequentialDownloader.downloadQueue.Count == 0)
            {
                if (SceneManager.GetActiveScene().name == "GameCore")
                {
                    refreshRequested = true;
                }
                else
                {
                    playlistDownloader.OnQueueClear();
                }
            }
        }

        private void OnMenuLoaded(Scene previousScene, Scene newScene)
        {
            if (refreshRequested && newScene.name == "MainMenu")
            {
                refreshRequested = false;
                playlistDownloader.OnQueueClear();
            }
        }
    }
}
