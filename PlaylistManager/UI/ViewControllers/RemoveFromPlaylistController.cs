using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using TMPro;
using PlaylistManager.Interfaces;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;
using HMUI;
using UnityEngine;

namespace PlaylistManager.UI
{
    class RemoveFromPlaylistController : IPreviewBeatmapLevelUpdater, IPlaylistManagerModal
    {
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelCollectionViewController levelCollectionViewController;
        private readonly PopupModalsController popupModalsController;
        private IPlaylistSong selectedPlaylistSong;

        [UIComponent("warning-message")]
        private readonly TextMeshProUGUI warningMessage;

        [UIComponent("modal")]
        private readonly ModalView modal;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        public bool parsed { get; private set; }

        public RemoveFromPlaylistController(StandardLevelDetailViewController standardLevelDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController, PopupModalsController popupModalsController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            this.popupModalsController = popupModalsController;
            parsed = false;
        }

        internal void Parse()
        {
        }

        internal void DisplayWarning()
        {
            popupModalsController.ShowYesNoModal(standardLevelDetailViewController.transform, string.Format("Are you sure you would like to remove {0} from the playlist?", selectedPlaylistSong.songName), RemoveSong);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {   
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = PlaylistLibUtils.playlistManager.GetAllPlaylists(true)[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            selectedPlaylist.Remove(selectedPlaylistSong);
            PlaylistLibUtils.playlistManager.GetManagerForPlaylist(selectedPlaylist).StorePlaylist(selectedPlaylist);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel) // This is needed as it is impossible to get the IPlaylistSong through IDifficultyBeatmapLevel
        {
            if (beatmapLevel is IPlaylistSong)
            {
                selectedPlaylistSong = (IPlaylistSong)beatmapLevel;
            }
        }

        public void ParentControllerDeactivated()
        {
            if (parsed && rootTransform != null && modalTransform != null)
            {
                modalTransform.transform.SetParent(rootTransform);
                modalTransform.position = modalPosition;
            }
        }
    }
}
