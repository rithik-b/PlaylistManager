using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using TMPro;
using PlaylistManager.Interfaces;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;
using HMUI;

namespace PlaylistManager.UI
{
    class RemoveFromPlaylistController : IPreviewBeatmapLevelUpdater
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;
        private IPlaylistSong selectedPlaylistSong;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        [UIComponent("modal")]
        private ModalView modal;

        internal bool parsed;
        RemoveFromPlaylistController(StandardLevelDetailViewController standardLevelDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            parsed = false;
        }

        internal void DisplayWarning()
        {
            modal.Show(true);
            warningMessage.text = string.Format("Are you sure you would like to remove\n{0} from the playlist?", selectedPlaylistSong.songName);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {   
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = PlaylistLibUtils.playlistManager.GetAllPlaylists()[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            selectedPlaylist.Remove(selectedPlaylistSong);
            PlaylistLibUtils.playlistManager.StorePlaylist(selectedPlaylist);
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.AnnotatedBeatmapLevelCollectionsViewController_SetData.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if (beatmapLevel is IPlaylistSong)
            {
                selectedPlaylistSong = (IPlaylistSong)beatmapLevel;
            }
        }

        internal void Parse()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.RemoveFromPlaylist.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
        }
    }
}
