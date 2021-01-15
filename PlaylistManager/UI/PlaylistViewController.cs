using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.Reflection;
using TMPro;
using UnityEngine;
using PlaylistManager.Interfaces;
using BeatSaberPlaylistsLib.Types;
using IPA.Utilities;

namespace PlaylistManager.UI
{
    class PlaylistViewController : ILevelCollectionUpdater
    {
        private LevelPackDetailViewController levelPackDetailViewController;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private LevelCollectionViewController levelCollectionViewController;

        // Currently selected map pack
        private IAnnotatedBeatmapLevelCollection levelCollection;

        [UIComponent("bg")]
        private Transform bgTransform;

        [UIComponent("warning-message")]
        private TextMeshProUGUI warningMessage;

        [UIComponent("ok-message")]
        private TextMeshProUGUI okMessage;

        [UIComponent("ok-modal")]
        private ModalView okModal;

        PlaylistViewController(LevelPackDetailViewController levelPackDetailViewController, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionViewController levelCollectionViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionViewController = levelCollectionViewController;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.PlaylistView.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
            bgTransform.gameObject.SetActive(false);
        }

        [UIAction("delete-click")]
        internal void DisplayWarning()
        {
            warningMessage.text = string.Format("Are you sure you would like to delete \n{0}?", levelCollection.collectionName);
        }

        [UIAction("delete-confirm")]
        internal void DeletePlaylist()
        {
            /*
            if (Playlist.DeletePlaylist(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection))
            {
                annotatedBeatmapLevelCollectionsViewController.SetData(HarmonyPatches.PlaylistCollectionOverride.otherCustomBeatmapLevelCollections, annotatedBeatmapLevelCollectionsViewController.selectedItemIndex - 1, false);
                IAnnotatedBeatmapLevelCollection selectedCollection = annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection;
                levelCollectionViewController.SetData(selectedCollection.beatmapLevelCollection, selectedCollection.collectionName, selectedCollection.coverImage, false, null);
                levelPackDetailViewController.SetData((IBeatmapLevelPack)selectedCollection);
                LevelPackUpdated();
            }
            else
            {
                okMessage.text = "There was an error deleting the Playlist";
                okModal.Show(true);
            }
            */
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection)
        {
            if (annotatedBeatmapLevelCollectionsViewController.isActiveAndEnabled && beatmapLevelCollection is Playlist)
            {
                bgTransform.gameObject.SetActive(true);
            }
            else
            {
                bgTransform.gameObject.SetActive(false);
            }
        }
    }
}
