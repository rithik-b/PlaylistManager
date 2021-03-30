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
        private readonly SelectLevelCategoryViewController selectLevelCategoryViewController;
        private readonly LevelSelectionNavigationController levelSelectionNavigationController;
        private readonly StandardLevelDetailViewController standardLevelDetailViewController;
        private readonly LobbyGameState lobbyGameState;

        private readonly List<ILevelCategoryUpdater> levelCategoryUpdaters;
        private readonly IStackedModalView stackedModalView;
        private readonly List<IRefreshable> refreshables;
        private readonly IMultiplayerGameStateUpdater multiplayerGameStateUpdater;
        private readonly IPlatformUserModel platformUserModel;

        internal PlaylistUIManager(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, SelectLevelCategoryViewController selectLevelCategoryViewController, LevelSelectionNavigationController levelSelectionNavigationController, 
            StandardLevelDetailViewController standardLevelDetailViewController, LobbyGameState lobbyGameState, List<ILevelCategoryUpdater> levelCategoryUpdaters, IStackedModalView stackedModalView, List<IRefreshable> refreshables,
            IMultiplayerGameStateUpdater multiplayerGameStateUpdater, IPlatformUserModel platformUserModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            this.levelSelectionNavigationController = levelSelectionNavigationController;
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.lobbyGameState = lobbyGameState;

            this.levelCategoryUpdaters = levelCategoryUpdaters;
            this.stackedModalView = stackedModalView;
            this.refreshables = refreshables;
            this.multiplayerGameStateUpdater = multiplayerGameStateUpdater;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            // Whenever a level category is selected
            selectLevelCategoryViewController.didSelectLevelCategoryEvent += SelectLevelCategoryViewController_didSelectLevelCategoryEvent;
            selectLevelCategoryViewController.didActivateEvent += SelectLevelCategoryViewController_didActivateEvent;
            selectLevelCategoryViewController.didDeactivateEvent += SelectLevelCategoryViewController_didDeactivateEvent;

            // When a stacked modal is dismissed
            stackedModalView.ModalDismissedEvent += StackedModalView_ModalDismissedEvent;

            // When the multiplayer state changes
            lobbyGameState.gameStateDidChangeEvent += LobbyGameState_gameStateDidChangeEvent;

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

            stackedModalView.ModalDismissedEvent -= StackedModalView_ModalDismissedEvent;

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

        private void StackedModalView_ModalDismissedEvent()
        {
            levelSelectionNavigationController.canvasGroup.alpha = 0.2f;
        }

        private void LobbyGameState_gameStateDidChangeEvent(MultiplayerGameState multiplayerGameState)
        {
            multiplayerGameStateUpdater.MultiplayerGameStateUpdated(multiplayerGameState);
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
