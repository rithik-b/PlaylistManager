using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using TMPro;
using UnityEngine;
using PlaylistManager.Interfaces;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;

namespace PlaylistManager.UI
{
    class RemoveFromPlaylistController : IPreviewBeatmapLevelUpdater
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;
        private IPlaylistSong selectedPlaylistSong;

        [UIComponent("remove-button")]
        private Transform removeButtonTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        RemoveFromPlaylistController(StandardLevelDetailViewController standardLevelDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.RemoveFromPlaylist.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
            removeButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to remove \n{0}\n from the playlist?", selectedPlaylistSong.songName);
        }

        [UIAction("delete-confirm")]
        internal void RemoveSong()
        {   
            BeatSaberPlaylistsLib.Types.IPlaylist selectedPlaylist = PlaylistLibUtils.playlistManager.GetAllPlaylists()[annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 2];
            selectedPlaylist.Remove((IPlaylistSong)selectedPlaylistSong);
            PlaylistLibUtils.playlistManager.StorePlaylist(selectedPlaylist);
            annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex, false);
            levelCollectionViewController.SetData(selectedPlaylist.beatmapLevelCollection, selectedPlaylist.collectionName, selectedPlaylist.coverImage, false, null);
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if(beatmapLevel is IPlaylistSong)
            {
                selectedPlaylistSong = (IPlaylistSong)beatmapLevel;
                removeButtonTransform.gameObject.SetActive(true);
            }
            else
            {
                removeButtonTransform.gameObject.SetActive(false);
            }
        }
    }
}
