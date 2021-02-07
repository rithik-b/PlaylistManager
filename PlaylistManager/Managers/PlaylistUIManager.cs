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
        readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        readonly SelectLevelCategoryViewController selectLevelCategoryViewController;
        readonly PlaylistViewController playlistViewController;
        readonly LevelPackDetailViewController levelPackDetailViewController;
        readonly ILevelCollectionUpdater levelCollectionUpdater;
        readonly IPlatformUserModel platformUserModel;
        readonly List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;
        readonly StandardLevelDetailViewController standardLevelDetailViewController;
        readonly List<IPlaylistManagerModal> playlistManagerModals;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController,
            LevelPackDetailViewController levelPackDetailViewController, PlaylistViewController playlistViewController, ILevelCollectionUpdater levelCollectionUpdater, List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters,
            StandardLevelDetailViewController standardLevelDetailViewController, List<IPlaylistManagerModal> playlistManagerModals, IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.playlistViewController = playlistViewController;
            this.levelPackDetailViewController = levelPackDetailViewController;
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
            levelPackDetailViewController.didActivateEvent += LevelPackDetailViewController_didActivateEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;

            _ = AssignAuthor();
        }

        public void Dispose()
        {
            standardLevelDetailViewController.didDeactivateEvent -= StandardLevelDetailViewController_didDeactivateEvent;
            standardLevelDetailViewController.didChangeContentEvent -= StandardLevelDetailViewController_didChangeContentEvent;

            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;
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

        private void LevelPackDetailViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            levelCollectionUpdater.LevelCollectionUpdated();
        }

        private void SelectLevelCategoryViewController_didSelectLevelCategoryEvent(SelectLevelCategoryViewController selectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            levelCollectionUpdater.LevelCategoryUpdated(levelCategory);
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
