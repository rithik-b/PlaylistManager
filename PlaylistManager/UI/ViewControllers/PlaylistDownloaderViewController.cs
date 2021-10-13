using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [UIComponent("download-list")]
        private readonly CustomCellListTableData customListTableData;

        [Inject]
        public void Construct(PlaylistDownloader playlistDownloader)
        {
            this.playlistDownloader = playlistDownloader;
        }

        public void Initialize()
        {
            //floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, Vector3.zero, Quaternion.identity);
            //floatingScreen.HighlightHandle = true;
            //floatingScreen.SetRootViewController(this, AnimationType.In);

            playlistDownloader.QueueUpdatedEvent += UpdateQueue;
        }

        public void Dispose()
        {
            if (floatingScreen != null && floatingScreen.gameObject != null)
            {
                Destroy(floatingScreen.gameObject);
            }

            playlistDownloader.QueueUpdatedEvent -= UpdateQueue;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            customListTableData.data = playlistDownloader.downloadQueue;
            UpdateQueue();
        }

        private void UpdateQueue()
        {
            if (customListTableData != null)
            {
                customListTableData.tableView.ReloadDataKeepingPosition();
            }
        }
    }
}
