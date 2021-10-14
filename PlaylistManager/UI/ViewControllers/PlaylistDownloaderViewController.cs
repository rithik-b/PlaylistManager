using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Utilities;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\PlaylistDownloaderView.bsml")]
    [ViewDefinition("PlaylistManager.UI.Views.PlaylistDownloaderView.bsml")]
    internal class PlaylistDownloaderViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private PlaylistDownloader playlistDownloader;
        private PopupModalsController popupModalsController;
        private FloatingScreen floatingScreen;
        private bool refreshRequested;

        [UIComponent("download-list")]
        private readonly CustomCellListTableData customListTableData;

        [Inject]
        public void Construct(PlaylistDownloader playlistDownloader, PopupModalsController popupModalsController)
        {
            this.playlistDownloader = playlistDownloader;
            this.popupModalsController = popupModalsController;
        }

        public void OnEnable()
        {
            if (playlistDownloader.PendingPopup != null)
            {
                popupModalsController.ShowYesNoModal(playlistDownloader.PendingPopup);
            }
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
            //floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, Vector3.zero, Quaternion.identity);
            //floatingScreen.HighlightHandle = true;
            //floatingScreen.SetRootViewController(this, AnimationType.In);

            playlistDownloader.PopupEvent += OnPopupRequested;
            playlistDownloader.QueueUpdatedEvent += UpdateQueue;
            SongCore_MenuLoaded.MenuLoadedEvent += OnMenuLoaded;
        }

        public void Dispose()
        {
            if (floatingScreen != null && floatingScreen.gameObject != null)
            {
                Destroy(floatingScreen.gameObject);
            }

            playlistDownloader.PopupEvent -= OnPopupRequested;
            playlistDownloader.QueueUpdatedEvent -= UpdateQueue;
            SongCore_MenuLoaded.MenuLoadedEvent -= OnMenuLoaded;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            customListTableData.data = playlistDownloader.downloadQueue;
            UpdateQueue();
        }

        private void OnPopupRequested()
        {
            if (playlistDownloader.PendingPopup != null)
            {
                playlistDownloader.PendingPopup.parent = transform;
                if (isActiveAndEnabled)
                {
                    popupModalsController.ShowYesNoModal(playlistDownloader.PendingPopup);
                }
            }
        }

        private void UpdateQueue()
        {
            if (customListTableData != null)
            {
                customListTableData.tableView.ReloadDataKeepingPosition();
            }

            if (playlistDownloader.downloadQueue.Count == 0)
            {
                if (!isActiveAndEnabled)
                {
                    refreshRequested = true;
                }
                else
                {
                    playlistDownloader.OnQueueClear();
                }
            }
        }

        private void OnMenuLoaded()
        {
            if (refreshRequested)
            {
                refreshRequested = false;
                playlistDownloader.OnQueueClear();
            }
        }
    }
}
