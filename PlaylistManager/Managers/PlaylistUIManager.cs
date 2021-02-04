using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using PlaylistManager.UI;
using PlaylistManager.HarmonyPatches;
using System.Threading.Tasks;
using PlaylistManager.Configuration;

namespace PlaylistManager.Managers
{
    internal class PlaylistUIManager : IInitializable, IDisposable
    {
        AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        SelectLevelCategoryViewController selectLevelCategoryViewController;
        PlaylistViewController playlistViewController;
        ILevelCollectionUpdater levelCollectionUpdater;
        IPlatformUserModel platformUserModel;

        List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;

        StandardLevelDetailViewController standardLevelDetailViewController;
        List<IPlaylistManagerModal> playlistManagerModals;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController, PlaylistViewController playlistViewController, ILevelCollectionUpdater levelCollectionUpdater,
            List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters, StandardLevelDetailViewController standardLevelDetailViewController, List<IPlaylistManagerModal> playlistManagerModals, IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.playlistViewController = playlistViewController;
            this.levelCollectionUpdater = levelCollectionUpdater;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.playlistManagerModals = playlistManagerModals;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            standardLevelDetailViewController.didDeactivateEvent += StandardLevelDetailViewController_didDeactivateEvent;
            standardLevelDetailViewController.didChangeContentEvent += StandardLevelDetailViewController_didChangeContentEvent;
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.didSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            _ = AssignAuthor();
        }

        public void Dispose()
        {
            standardLevelDetailViewController.didDeactivateEvent -= StandardLevelDetailViewController_didDeactivateEvent;
            standardLevelDetailViewController.didChangeContentEvent -= StandardLevelDetailViewController_didChangeContentEvent;
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.didSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;
            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
        }

        private async Task AssignAuthor()
        {
            if (PluginConfig.Instance.AuthorName == null || PluginConfig.Instance.AuthorName == nameof(PlaylistManager))
            {
                UserInfo user = await platformUserModel.GetUserInfo();
                PluginConfig.Instance.AuthorName = user?.userName ?? nameof(PlaylistManager);
            }
            else
            {
                PluginConfig.Instance.AuthorName = PluginConfig.Instance.AuthorName;
            }
        }

        private void LevelCollectionViewController_didSelectLevelEvent(IPreviewBeatmapLevel beatmapLevel)
        {
            foreach (IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater in previewBeatmapLevelUpdaters)
            {
                previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(beatmapLevel);
            }
        }

        private void DidSelectAnnotatedBeatmapLevelCollectionEvent(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection)
        {
            levelCollectionUpdater.LevelCollectionUpdated();
        }

        private void SelectLevelCategoryViewController_didSelectLevelCategoryEvent(SelectLevelCategoryViewController arg1, SelectLevelCategoryViewController.LevelCategory arg2)
        {
            levelCollectionUpdater.LevelCollectionUpdated();
        }

        private void StandardLevelDetailViewController_didChangeContentEvent(StandardLevelDetailViewController standardLevelDetailViewController, StandardLevelDetailViewController.ContentType contentType)
        {
            if (contentType != StandardLevelDetailViewController.ContentType.OwnedAndReady)
            {
                foreach (IPlaylistManagerModal playlistManagerModal in playlistManagerModals)
                {
                    playlistManagerModal.ParentControllerDeactivated();
                }
            }
        }

        private void StandardLevelDetailViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            foreach (IPlaylistManagerModal playlistManagerModal in playlistManagerModals)
            {
                playlistManagerModal.ParentControllerDeactivated();
            }
        }
    }
}
