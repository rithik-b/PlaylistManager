using System;
using System.Collections.Generic;
using Zenject;
using PlaylistManager.Interfaces;
using System.Threading.Tasks;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;

namespace PlaylistManager.Managers
{
    internal class PlaylistUIManager : IInitializable, IDisposable
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly SelectLevelCategoryViewController selectLevelCategoryViewController;

        private readonly List<ILevelCategoryUpdater> levelCategoryUpdaters;
        private readonly List<IRefreshable> refreshables;
        private readonly IPlatformUserModel platformUserModel;

        internal PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController,
            StandardLevelDetailViewController standardLevelDetailViewController, List<ILevelCategoryUpdater> levelCategoryUpdaters, List<IRefreshable> refreshables, IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.standardLevelDetailViewController = standardLevelDetailViewController;

            this.levelCategoryUpdaters = levelCategoryUpdaters;
            this.refreshables = refreshables;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            // Whenever a level category is selected
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent += SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent += SelectLevelCategoryViewController_didDeactivateEvent;

            // Whenever a refresh is requested
            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested += PlaylistManager_PlaylistsRefreshRequested;

            // For assigning playlist author
            _ = AssignAuthor();
        }

        public void Dispose()
        {
            selectLevelCategoryViewController.didSelectLevelCategoryEvent -= SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent -= SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent -= SelectLevelCategoryViewController_didDeactivateEvent;

            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested -= PlaylistManager_PlaylistsRefreshRequested;
        }

        private void SelectLevelCategoryViewController_didSelectLevelCategoryEvent(SelectLevelCategoryViewController selectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            foreach (ILevelCategoryUpdater levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(levelCategory, false);
            }
        }

        private void SelectLevelCategoryViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            foreach (ILevelCategoryUpdater levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(selectLevelCategoryViewController.selectedLevelCategory, true);
            }
        }

        private void SelectLevelCategoryViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            foreach (ILevelCategoryUpdater levelCategoryUpdater in levelCategoryUpdaters)
            {
                levelCategoryUpdater.LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory.None, false);
            }
        }

        private void PlaylistManager_PlaylistsRefreshRequested(object sender, string requester)
        {
            Plugin.Log.Info("Refresh requested by: " + requester);
            foreach (IRefreshable refreshable in refreshables)
            {
                refreshable.Refresh();
            }
        }

        private async Task AssignAuthor()
        {
            if (PluginConfig.Instance.AutomaticAuthorName)
            {
                UserInfo user = await platformUserModel.GetUserInfo();
                if (PluginConfig.Instance.AuthorName == null && user == null)
                {
                    PluginConfig.Instance.AuthorName = nameof(PlaylistManager);
                }
                else
                {
                    PluginConfig.Instance.AuthorName = user?.userName ?? PluginConfig.Instance.AuthorName;
                }
            }
            else
            {
                PluginConfig.Instance.AuthorName = PluginConfig.Instance.AuthorName;
            }
        }
    }
}
