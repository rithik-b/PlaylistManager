using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlaylistManager.UI
{
    public class FoldersViewController : IInitializable, IDisposable, INotifyPropertyChanged, ILevelCollectionsTableUpdater, ILevelCategoryUpdater, IPMRefreshable
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly LevelSelectionNavigationController levelSelectionNavigationController;
        private readonly PopupModalsController popupModalsController;
        private readonly HoverHintController hoverHintController;
        private BeatmapLevelsModel beatmapLevelsModel;

        private FloatingScreen floatingScreen;
        private readonly Sprite levelPacksIcon;
        private readonly Sprite customPacksIcon;
        private readonly Sprite playlistsIcon;
        private readonly Sprite foldersIcon;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;

        private BeatSaberPlaylistsLib.PlaylistManager currentParentManager;
        private List<BeatSaberPlaylistsLib.PlaylistManager> currentManagers;
        private FolderMode folderMode;

        [UIComponent("root")]
        private RectTransform rootTransform;

        [UIComponent("back-rect")]
        private RectTransform backTransform;

        [UIComponent("rename-button")]
        private Button renameButton;

        [UIComponent("delete-button")]
        private Button deleteButton;

        [UIComponent("folder-list")]
        public CustomListTableData customListTableData;

        public FoldersViewController(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, MainFlowCoordinator mainFlowCoordinator, 
            LevelSelectionNavigationController levelSelectionNavigationController, PopupModalsController popupModalsController, HoverHintController hoverHintController,
            BeatmapLevelsModel beatmapLevelsModel)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.levelSelectionNavigationController = levelSelectionNavigationController;
            this.popupModalsController = popupModalsController;
            this.hoverHintController = hoverHintController;
            this.beatmapLevelsModel = beatmapLevelsModel;

            levelPacksIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.LevelPacks.png");
            customPacksIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.CustomPacks.png");
            playlistsIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Playlists.png");
            foldersIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Folders.png");

            folderMode = FolderMode.None;
        }

        public void Initialize()
        {
            floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(75, 25), false, new Vector3(0f, 0.2f, 2.5f), new Quaternion(0, 0, 0, 0));
            floatingScreen.transform.eulerAngles = new Vector3(60, 0, 0);
            floatingScreen.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);

            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.FoldersView.bsml"), floatingScreen.gameObject, this);
            LevelFilteringNavigationController_ShowPacksInChildController.AllPacksViewSelectedEvent += LevelFilteringNavigationController_ShowPacksInChildController_AllPacksViewSelectedEvent;
        }

        public void Dispose()
        {
            LevelFilteringNavigationController_ShowPacksInChildController.AllPacksViewSelectedEvent -= LevelFilteringNavigationController_ShowPacksInChildController_AllPacksViewSelectedEvent;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            rootTransform.gameObject.SetActive(false);
            rootTransform.gameObject.name = "PlaylistManagerFoldersView";

            ScrollView scrollView = customListTableData.tableView.GetComponent<ScrollView>();
        }

        public void SetupDimensions()
        {
            if (!(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf() is MultiplayerLevelSelectionFlowCoordinator))
            {
                floatingScreen.transform.position = new Vector3(0f, 0.1f, 2.5f);
                floatingScreen.transform.eulerAngles = new Vector3(75, 0, 0);
                floatingScreen.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            }
            else
            {
                Vector3 foldersPosition = levelSelectionNavigationController.transform.position;
                foldersPosition.y += 0.73f;
                floatingScreen.transform.eulerAngles = new Vector3(0, 0, 0);
                floatingScreen.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                floatingScreen.transform.position = foldersPosition;
            }
        }

        private void SetupList(BeatSaberPlaylistsLib.PlaylistManager currentParentManager, bool setBeatmapLevelCollections = true)
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            this.currentParentManager = currentParentManager;

            if (currentParentManager == null)
            {
                CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo("Level Packs", icon: levelPacksIcon);
                customListTableData.data.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("Custom Songs", icon: customPacksIcon);
                customListTableData.data.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("Playlists", icon: playlistsIcon);
                customListTableData.data.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("Folders", icon: foldersIcon);
                customListTableData.data.Add(customCellInfo);

                backTransform.gameObject.SetActive(false);
            }
            else
            {
                currentManagers = currentParentManager.GetChildManagers().ToList();
                foreach (var childManager in currentManagers)
                {
                    var folderName = Path.GetFileName(childManager.PlaylistPath);
                    CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: PlaylistLibUtils.DrawFolderIcon(folderName));
                    customListTableData.data.Add(customCellInfo);
                }

                backTransform.gameObject.SetActive(true);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));

                // If root, can't rename or delete
                if (currentParentManager.Parent == null)
                {
                    renameButton.interactable = false;
                    deleteButton.interactable = false;
                }
                else
                {
                    renameButton.interactable = true;
                    deleteButton.interactable = true;
                }

                if (setBeatmapLevelCollections)
                {
                    IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections = currentParentManager.GetAllPlaylists(false);
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                }
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            if (currentParentManager == null)
            {
                customListTableData.tableView.SelectCellWithIdx(0);

                // Add hover hint
                TableCell[] visibleCells = customListTableData.tableView.visibleCells.ToArray();
                for (int i = 0; i < visibleCells.Length; i++)
                {
                    HoverHint hoverHint = visibleCells[i].GetComponent<HoverHint>();
                    if (hoverHint == null)
                    {
                        hoverHint = visibleCells[i].gameObject.AddComponent<HoverHint>();
                        Accessors.HoverHintControllerAccessor(ref hoverHint) = hoverHintController;
                    }
                    else
                    {
                        hoverHint.enabled = true;
                    }
                    hoverHint.text = customListTableData.data[i].text;
                }

                if (setBeatmapLevelCollections)
                {
                    Select(customListTableData.tableView, 0);
                }
            }
            else
            {
                // Disable hover hint
                TableCell[] visibleCells = customListTableData.tableView.visibleCells.ToArray();
                for (int i = 0; i < visibleCells.Length; i++)
                {
                    HoverHint hoverHint = visibleCells[i].GetComponent<HoverHint>();
                    if (hoverHint != null)
                    {
                        hoverHint.enabled = false;
                    }
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftButtonEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightButtonEnabled)));
        }

        [UIAction("folder-select")]
        private void Select(TableView _, int selectedCellIndex)
        {
            if (currentParentManager == null) // If we are at root
            {
                if (selectedCellIndex == 0)
                {
                    IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections = Accessors.CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks.Concat(PlaylistLibUtils.playlistManager.GetAllPlaylists(true)).ToArray();
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                    folderMode = FolderMode.AllPacks;

                }
                else if (selectedCellIndex == 1)
                {
                    IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections = Accessors.CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks;
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                    folderMode = FolderMode.CustomPacks;
                }
                else if (selectedCellIndex == 2)
                {
                    IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections = PlaylistLibUtils.playlistManager.GetAllPlaylists(true);
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                    folderMode = FolderMode.Playlists;
                }
                else if (selectedCellIndex == 3)
                {
                    SetupList(currentParentManager: PlaylistLibUtils.playlistManager);
                    folderMode = FolderMode.Folders;
                }
            }
            else
            {
                SetupList(currentParentManager: currentManagers[selectedCellIndex]);
            }
        }

        [UIAction("back-button-click")]
        private void BackButtonClicked()
        {
            if (currentParentManager == null)
            {
                return;
            }
            SetupList(currentParentManager: currentParentManager.Parent);
        }

        #region Create Folder

        [UIAction("create-folder")]
        private void CreateFolder()
        {
            popupModalsController.ShowKeyboard(levelSelectionNavigationController.transform, CreateKeyboardEnter);
        }

        private void CreateKeyboardEnter(string folderName)
        {
            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                BeatSaberPlaylistsLib.PlaylistManager childManager = currentParentManager.CreateChildManager(folderName);

                if (currentManagers.Contains(childManager))
                {
                    popupModalsController.ShowOkModal(levelSelectionNavigationController.transform, "\"" + folderName + "\" already exists! Please use a different name.", null);
                }
                else
                {
                    CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: PlaylistLibUtils.DrawFolderIcon(folderName));
                    customListTableData.data.Add(customCellInfo);
                    customListTableData.tableView.ReloadData();
                    customListTableData.tableView.ClearSelection();
                    currentManagers.Add(childManager);
                }
            }
        }

        #endregion

        #region Rename Folder

        [UIAction("rename-folder")]
        private void RenameButtonClicked()
        {
            popupModalsController.ShowKeyboard(levelSelectionNavigationController.transform, RenameKeyboardEnter, keyboardText: Path.GetFileName(currentParentManager.PlaylistPath));
        }

        private void RenameKeyboardEnter(string folderName)
        {
            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                if (folderName != Path.GetFileName(currentParentManager.PlaylistPath))
                {
                    currentParentManager.RenameManager(folderName);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
                }
            }
        }

        #endregion

        #region Delete Folder

        [UIAction("delete-folder")]
        private void DeleteButtonClicked()
        {
            popupModalsController.ShowYesNoModal(levelSelectionNavigationController.transform, string.Format("Are you sure you want to delete {0} along with all playlists and subfolders?", Path.GetFileName(currentParentManager.PlaylistPath)), DeleteConfirm);
        }

        private void DeleteConfirm()
        {
            currentParentManager.Parent.DeleteChildManager(currentParentManager);
            BackButtonClicked();
        }

        #endregion

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory, bool viewControllerActivated)
        {
            if (levelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                rootTransform.gameObject.SetActive(true);
                SetupDimensions();
                if (viewControllerActivated)
                {
                    SetupList(currentParentManager, false);
                }
            }
            else
            {
                rootTransform.gameObject.SetActive(false);
                folderMode = FolderMode.None;
            }
        }

        private void LevelFilteringNavigationController_ShowPacksInChildController_AllPacksViewSelectedEvent()
        {
            SetupList(null, false);
            folderMode = FolderMode.AllPacks;
        }

        public void Refresh()
        {
            if (folderMode == FolderMode.AllPacks)
            {
                IBeatmapLevelPack[] annotatedBeatmapLevelCollections = Accessors.CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks.Concat(PlaylistLibUtils.playlistManager.GetAllPlaylists(true)).ToArray();
                int indexToSelect = annotatedBeatmapLevelCollections.IndexOf(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection);
                if (indexToSelect != -1)
                {
                    annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                }
            }
            else if (folderMode == FolderMode.Playlists)
            {
                BeatSaberPlaylistsLib.Types.IPlaylist[] annotatedBeatmapLevelCollections = PlaylistLibUtils.playlistManager.GetAllPlaylists(true);
                int indexToSelect = annotatedBeatmapLevelCollections.IndexOf(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection);
                if (indexToSelect != -1)
                {
                    annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                }
            }
            else if (folderMode == FolderMode.Folders)
            {
                BeatSaberPlaylistsLib.Types.IPlaylist[] annotatedBeatmapLevelCollections = currentParentManager.GetAllPlaylists(false);
                int indexToSelect = annotatedBeatmapLevelCollections.IndexOf(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelCollection);
                if (indexToSelect != -1)
                {
                    annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                }
                SetupList(currentParentManager, false);
            }
        }

        [UIValue("folder-text")]
        private string FolderText
        {
            get
            {
                if (currentParentManager == null || !Directory.Exists(currentParentManager.PlaylistPath))
                {
                    return "";
                }
                else
                {
                    string folderName = Path.GetFileName(currentParentManager.PlaylistPath);
                    if (folderName.Length > 15)
                    {
                        return folderName.Substring(0, 15) + "...";
                    }
                    return folderName;
                }
            }
        }

        [UIValue("left-button-enabled")]
        private bool LeftButtonEnabled
        {
            get => customListTableData != null && customListTableData.data.Count > 4;
        }

        [UIValue("right-button-enabled")]
        private bool RightButtonEnabled
        {
            get => customListTableData != null && customListTableData.data.Count > 4;
        }
    }

    public enum FolderMode
    {
        None,
        AllPacks,
        CustomPacks,
        Playlists,
        Folders
    }
}
