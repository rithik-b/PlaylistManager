﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Interfaces;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    class PlaylistViewButtonsController : IInitializable, ILevelCollectionUpdater
    {
        private LevelPackDetailViewController levelPackDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private PlaylistViewController playlistViewController;

        [UIComponent("bg")]
        private Transform bgTransform;

        [UIComponent("sync-button")]
        private Transform syncButtonTransform;

        PlaylistViewButtonsController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, PlaylistViewController playlistViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.playlistViewController = playlistViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistViewButtons.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            syncButtonTransform.transform.localScale *= 0.08f;
            syncButtonTransform.gameObject.SetActive(false);
            bgTransform.gameObject.SetActive(false);
        }

        [UIAction("delete-click")]
        internal void OnDelete()
        {
            if(!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            playlistViewController.DisplayWarning();
        }

        [UIAction("download-click")]
        internal async System.Threading.Tasks.Task OnDownload()
        {
            if (!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            await playlistViewController.DownloadPlaylistAsync();
        }

        [UIAction("sync-click")]
        internal async System.Threading.Tasks.Task OnSync()
        {
            if (!playlistViewController.parsed)
            {
                playlistViewController.Parse();
            }
            await playlistViewController.SyncPlaylistAsync();
        }

        public void LevelCollectionUpdated()
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection is Playlist)
            {
                bgTransform.gameObject.SetActive(true);
                var customData = ((Playlist)annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection).CustomData;
                if (customData != null && customData.ContainsKey("syncURL"))
                {
                    syncButtonTransform.gameObject.SetActive(true);
                }
                else
                {
                    syncButtonTransform.gameObject.SetActive(false);
                }
            }
            else
            {
                bgTransform.gameObject.SetActive(false);
            }
        }
    }
}
