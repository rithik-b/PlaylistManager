using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
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
    public class FoldersViewController : IInitializable, ILevelCategoryUpdater, INotifyPropertyChanged, ILevelCollectionsTableUpdater, IDisposable
    {
        private readonly HMUI.Screen bottomScreen;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly PopupModalsController popupModalsController;
        private readonly Sprite customSongsCover;
        private BeatmapLevelsModel beatmapLevelsModel;

        public event PropertyChangedEventHandler PropertyChanged;
        public event System.Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;

        private BeatSaberPlaylistsLib.PlaylistManager currentParentManager;
        private List<BeatSaberPlaylistsLib.PlaylistManager> currentManagers;

        public static readonly FieldAccessor<HierarchyManager, ScreenSystem>.Accessor ScreenSystemAccessor = FieldAccessor<HierarchyManager, ScreenSystem>.GetAccessor("_screenSystem");
        public static readonly FieldAccessor<CustomLevelLoader, Sprite>.Accessor DefaultPackCoverAccessor = FieldAccessor<CustomLevelLoader, Sprite>.GetAccessor("_defaultPackCover");
        public static readonly FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.Accessor CustomLevelPackCollectionAccessor = FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.GetAccessor("_customLevelPackCollection");

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

        public FoldersViewController(HierarchyManager hierarchyManager, LevelCollectionNavigationController levelCollectionNavigationController, PopupModalsController popupModalsController, CustomLevelLoader customLevelLoader, BeatmapLevelsModel beatmapLevelsModel)
        {
            ScreenSystem screenSystem = ScreenSystemAccessor(ref hierarchyManager);
            bottomScreen = screenSystem.bottomScreen;

            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.popupModalsController = popupModalsController;
            this.beatmapLevelsModel = beatmapLevelsModel;

            customSongsCover = DefaultPackCoverAccessor(ref customLevelLoader);
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.FoldersView.bsml"), bottomScreen.gameObject, this);
            rootTransform.gameObject.SetActive(false);
            rootTransform.gameObject.name = "PlaylistManagerFoldersView";
        }

        private void SetupList(BeatSaberPlaylistsLib.PlaylistManager currentParentManager, bool viewControllerActivated = false)
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            this.currentParentManager = currentParentManager;

            if (currentParentManager == null)
            {
                CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo("Level Packs", icon: BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.LevelPacks.png"));
                customListTableData.data.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("Custom Songs", icon: customSongsCover);
                customListTableData.data.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("Folders", icon: BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.FolderIcon.png"));
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

                if (!viewControllerActivated)
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
                Select(customListTableData.tableView, 0);
            }
        }

        [UIAction("folder-select")]
        private void Select(TableView _, int selectedCellIndex)
        {
            if (currentParentManager == null) // If we are at root
            {
                if (selectedCellIndex == 0)
                {
                    IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections = CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks.Concat(PlaylistLibUtils.playlistManager.GetAllPlaylists(true)).ToArray();
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);

                }
                else if (selectedCellIndex == 1)
                {
                    IAnnotatedBeatmapLevelCollection[] annotatedBeatmapLevelCollections = CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks;
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                }
                else if (selectedCellIndex == 2)
                {
                    SetupList(currentParentManager: PlaylistLibUtils.playlistManager);
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
            popupModalsController.ShowKeyboard(levelCollectionNavigationController.transform, CreateKeyboardEnter);
        }

        private void CreateKeyboardEnter(string folderName)
        {
            if (!string.IsNullOrEmpty(folderName))
            {
                folderName = string.Join("_", folderName.Replace("/", "").Replace("\\", "").Split(' '));
                BeatSaberPlaylistsLib.PlaylistManager childManager = currentParentManager.CreateChildManager(folderName);

                CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: PlaylistLibUtils.DrawFolderIcon(folderName));
                customListTableData.data.Add(customCellInfo);
                customListTableData.tableView.ReloadData();
                customListTableData.tableView.ClearSelection();
                currentManagers.Add(childManager);
            }
        }

        #endregion

        #region Rename Folder

        [UIAction("rename-folder")]
        private void RenameButtonClicked()
        {
            popupModalsController.ShowKeyboard(levelCollectionNavigationController.transform, RenameKeyboardEnter);
        }

        private void RenameKeyboardEnter(string folderName)
        {
            if (!string.IsNullOrEmpty(folderName))
            {
                folderName = folderName.Replace("/", "").Replace("\\", "");
                currentParentManager.RenameManager(folderName);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
            }
        }

        #endregion

        #region Delete Folder

        [UIAction("delete-folder")]
        private void DeleteButtonClicked()
        {
            popupModalsController.ShowYesNoModal(levelCollectionNavigationController.transform, string.Format("Are you sure you want to delete {0} along with all playlists and subfolders?", Path.GetFileName(currentParentManager.PlaylistPath)), DeleteConfirm);
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
                bottomScreen.gameObject.SetActive(true);
                rootTransform.gameObject.SetActive(true);
                if (viewControllerActivated)
                {
                    SetupList(currentParentManager, true);
                }
                else
                {
                    AnnotatedBeatmapLevelCollectionsViewController_SetData.SetDataEvent += AnnotatedBeatmapLevelCollectionsViewController_SetData_SetDataEvent;
                }
            }
            else
            {
                rootTransform.gameObject.SetActive(false);
            }
        }

        private void AnnotatedBeatmapLevelCollectionsViewController_SetData_SetDataEvent()
        {
            AnnotatedBeatmapLevelCollectionsViewController_SetData.SetDataEvent -= AnnotatedBeatmapLevelCollectionsViewController_SetData_SetDataEvent;
            SetupList(currentParentManager);
        }

        public void Dispose()
        {
            AnnotatedBeatmapLevelCollectionsViewController_SetData.SetDataEvent -= AnnotatedBeatmapLevelCollectionsViewController_SetData_SetDataEvent;
        }

        [UIValue("folder-text")]
        private string FolderText
        {
            get => currentParentManager == null ? "" : Path.GetFileName(currentParentManager.PlaylistPath);
        }
    }
}
