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
        readonly LevelPackDetailViewController levelPackDetailViewController;
        readonly StandardLevelDetailViewController standardLevelDetailViewController;
        readonly PlaylistViewController playlistViewController;

        readonly ILevelCollectionUpdater levelCollectionUpdater;
        readonly List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters;
        readonly List<IPlaylistManagerModal> playlistManagerModals;
        readonly IPlatformUserModel platformUserModel;

        PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController,
            LevelPackDetailViewController levelPackDetailViewController, StandardLevelDetailViewController standardLevelDetailViewController, PlaylistViewController playlistViewController,
            ILevelCollectionUpdater levelCollectionUpdater, List<IPreviewBeatmapLevelUpdater> previewBeatmapLevelUpdaters, List<IPlaylistManagerModal> playlistManagerModals, IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.playlistViewController = playlistViewController;

            this.levelCollectionUpdater = levelCollectionUpdater;
            this.previewBeatmapLevelUpdaters = previewBeatmapLevelUpdaters;
            this.playlistManagerModals = playlistManagerModals;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            // Fixing modal pop
            standardLevelDetailViewController.didDeactivateEvent += StandardLevelDetailViewController_didDeactivateEvent;
            standardLevelDetailViewController.didChangeContentEvent += StandardLevelDetailViewController_didChangeContentEvent;

            // Whenever a beatmap level collection is selected
            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += DidSelectAnnotatedBeatmapLevelCollectionEvent;
            levelPackDetailViewController.didActivateEvent += LevelPackDetailViewController_didActivateEvent;

            // Whenever a level category is selected
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;

            // Whenever a level is selected
            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;

            // For assigning playlist author
            _ = AssignAuthor();
        }

        public void Dispose()
        {
            standardLevelDetailViewController.didDeactivateEvent -= StandardLevelDetailViewController_didDeactivateEvent;
            standardLevelDetailViewController.didChangeContentEvent -= StandardLevelDetailViewController_didChangeContentEvent;

            playlistViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent -= DidSelectAnnotatedBeatmapLevelCollectionEvent;

            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;

            LevelCollectionTableView_HandleDidSelectRowEvent.DidSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;
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

        private void LevelCollectionViewController_didSelectLevelEvent(IPreviewBeatmapLevel beatmapLevel)
        {
            foreach (IPreviewBeatmapLevelUpdater previewBeatmapLevelUpdater in previewBeatmapLevelUpdaters)
            {
                previewBeatmapLevelUpdater.PreviewBeatmapLevelUpdated(beatmapLevel);
            }
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
    }
}
