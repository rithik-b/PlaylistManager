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
using IPA.Loader;
using PlaylistManager.Types;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlaylistManager.UI
{
    public class FoldersViewController : IInitializable, IDisposable, INotifyPropertyChanged, ILevelCollectionsTableUpdater, ILevelCategoryUpdater, IPMRefreshable, TableView.IDataSource
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly LevelSelectionNavigationController levelSelectionNavigationController;
        private readonly PopupModalsController popupModalsController;
        private readonly HoverHintController hoverHintController;
        private readonly BeatmapLevelsModel beatmapLevelsModel;
        private readonly PluginMetadata pluginMetadata;
        private readonly BSMLParser bsmlParser;

        private FloatingScreen floatingScreen;
        private readonly Sprite levelPacksSprite;
        private readonly Sprite customPacksSprite;
        private readonly Sprite playlistsSprite;
        private readonly Sprite foldersSprite;
        private readonly Sprite folderIcon;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<IReadOnlyList<BeatmapLevelPack>, int> LevelCollectionTableViewUpdatedEvent;
        public event Action<BeatSaberPlaylistsLib.PlaylistManager> ParentManagerUpdatedEvent;

        private readonly List<CustomListTableData.CustomCellInfo> tableCells;
        private BeatSaberPlaylistsLib.PlaylistManager _currentParentManager;
        private List<BeatSaberPlaylistsLib.PlaylistManager> currentManagers;
        private FolderMode folderMode;

        public BeatSaberPlaylistsLib.PlaylistManager CurrentParentManager
        {
            get => _currentParentManager;
            private set
            {
                _currentParentManager = value;
                ParentManagerUpdatedEvent?.Invoke(value);
            }
        }

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
            BeatmapLevelsModel beatmapLevelsModel, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.levelSelectionNavigationController = levelSelectionNavigationController;
            this.popupModalsController = popupModalsController;
            this.hoverHintController = hoverHintController;
            this.beatmapLevelsModel = beatmapLevelsModel;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;

            levelPacksSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.LevelPacks.png");
            customPacksSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.CustomPacks.png");
            playlistsSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Playlists.png");
            foldersSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Folders.png");
            folderIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.FolderIcon.png");

            tableCells = new List<CustomListTableData.CustomCellInfo>();
            folderMode = FolderMode.None;
        }

        public void Initialize()
        {
            floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(75, 25), false, new Vector3(0f, 0.2f, 2.5f), new Quaternion(0, 0, 0, 0));
            var transform = floatingScreen.transform;
            transform.eulerAngles = new Vector3(60, 0, 0);
            transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);

            bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.FoldersView.bsml"), floatingScreen.gameObject, this);
            LevelFilteringNavigationController_ShowPacksInChildController.AllPacksViewSelectedEvent += LevelFilteringNavigationController_ShowPacksInChildController_AllPacksViewSelectedEvent;
        }

        public void Dispose()
        {
            LevelFilteringNavigationController_ShowPacksInChildController.AllPacksViewSelectedEvent -= LevelFilteringNavigationController_ShowPacksInChildController_AllPacksViewSelectedEvent;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            var gameObject = rootTransform.gameObject;
            gameObject.SetActive(false);
            gameObject.name = "PlaylistManagerFoldersView";
            customListTableData.tableView.SetDataSource(this, false);
        }

        public void SetupDimensions()
        {
            if (!(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf() is MultiplayerLevelSelectionFlowCoordinator))
            {
                var transform = floatingScreen.transform;
                transform.position = new Vector3(0f, 0.1f, 2.5f);
                transform.eulerAngles = new Vector3(75, 0, 0);
                transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            }
            else
            {
                var foldersPosition = levelSelectionNavigationController.transform.position;
                foldersPosition.y += 0.73f;
                var transform = floatingScreen.transform;
                transform.eulerAngles = new Vector3(0, 0, 0);
                transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                transform.position = foldersPosition;
            }
        }

        private void SetupList(BeatSaberPlaylistsLib.PlaylistManager currentParentManager, bool setBeatmapLevelCollections = true)
        {
            customListTableData.tableView.ClearSelection();
            tableCells.Clear();
            CurrentParentManager = currentParentManager;

            if (currentParentManager == null)
            {
                var customCellInfo = new CustomListTableData.CustomCellInfo("","Level Packs", levelPacksSprite);
                tableCells.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("","Custom Songs", customPacksSprite);
                tableCells.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("","Playlists", playlistsSprite);
                tableCells.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("","Folders", foldersSprite);
                tableCells.Add(customCellInfo);

                backTransform.gameObject.SetActive(false);
            }
            else
            {
                currentManagers = currentParentManager.GetChildManagers().ToList();
                foreach (var childManager in currentManagers)
                {
                    var folderName = Path.GetFileName(childManager.PlaylistPath);
                    var customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: folderIcon);
                    tableCells.Add(customCellInfo);
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
                    IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections = currentParentManager.GetAllPlaylists(false).Select(p => p.PlaylistLevelPack).ToArray();
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                }
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            if (currentParentManager == null)
            {
                customListTableData.tableView.SelectCellWithIdx(0);

                // Add hover hint
                var visibleCells = customListTableData.tableView.visibleCells.ToArray();
                for (var i = 0; i < visibleCells.Length; i++)
                {
                    var hoverHint = visibleCells[i].GetComponent<HoverHint>();
                    if (hoverHint == null)
                    {
                        hoverHint = visibleCells[i].gameObject.AddComponent<HoverHint>();
                        Accessors.HoverHintControllerAccessor(ref hoverHint) = hoverHintController;
                    }
                    else
                    {
                        hoverHint.enabled = true;
                    }
                    hoverHint.text = tableCells[i].subtext;
                }

                if (setBeatmapLevelCollections)
                {
                    Select(customListTableData.tableView, 0);
                }
            }
            else
            {
                // Disable hover hint
                var visibleCells = customListTableData.tableView.visibleCells.ToArray();
                for (var i = 0; i < visibleCells.Length; i++)
                {
                    var hoverHint = visibleCells[i].GetComponent<HoverHint>();
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
            if (CurrentParentManager == null) // If we are at root
            {
                if (selectedCellIndex == 0)
                {
                    IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections = beatmapLevelsModel._customLevelsRepository.beatmapLevelPacks.Concat(PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks()).ToArray();
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                    folderMode = FolderMode.AllPacks;

                }
                else if (selectedCellIndex == 1)
                {
                    IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections = beatmapLevelsModel._customLevelsRepository.beatmapLevelPacks;
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                    folderMode = FolderMode.CustomPacks;
                }
                else if (selectedCellIndex == 2)
                {
                    IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections = PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks();
                    LevelCollectionTableViewUpdatedEvent?.Invoke(annotatedBeatmapLevelCollections, 0);
                    folderMode = FolderMode.Playlists;
                }
                else if (selectedCellIndex == 3)
                {
                    SetupList(PlaylistLibUtils.playlistManager);
                    folderMode = FolderMode.Folders;
                }
            }
            else
            {
                SetupList(currentManagers[selectedCellIndex]);
            }
        }

        [UIAction("back-button-click")]
        private void BackButtonClicked()
        {
            if (CurrentParentManager == null)
            {
                return;
            }
            SetupList(currentParentManager: CurrentParentManager.Parent);
        }

        #region Create Folder

        [UIAction("create-folder")]
        private void CreateFolder()
        {
            popupModalsController.ShowKeyboard(levelSelectionNavigationController.transform, CreateKeyboardEnter);
        }

        private void CreateKeyboardEnter(string folderName)
        {
            if (CurrentParentManager == null)
            {
                return;
            }

            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                var childManager = CurrentParentManager.CreateChildManager(folderName);

                if (currentManagers.Contains(childManager))
                {
                    popupModalsController.ShowOkModal(levelSelectionNavigationController.transform, "\"" + folderName + "\" already exists! Please use a different name.", null);
                }
                else
                {
                    var customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                    tableCells.Add(customCellInfo);
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
            popupModalsController.ShowKeyboard(levelSelectionNavigationController.transform, RenameKeyboardEnter, keyboardText: Path.GetFileName(CurrentParentManager.PlaylistPath));
        }

        private void RenameKeyboardEnter(string folderName)
        {
            if (CurrentParentManager?.Parent == null)
            {
                return;
            }

            folderName = folderName.Replace("/", "").Replace("\\", "").Replace(".", "");
            if (!string.IsNullOrEmpty(folderName))
            {
                if (folderName != Path.GetFileName(CurrentParentManager.PlaylistPath))
                {
                    CurrentParentManager.RenameManager(folderName);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
                }
            }
        }

        #endregion

        #region Delete Folder

        [UIAction("delete-folder")]
        private void DeleteButtonClicked()
        {
            popupModalsController.ShowYesNoModal(levelSelectionNavigationController.transform, string.Format("Are you sure you want to delete {0} along with all playlists and subfolders?", Path.GetFileName(CurrentParentManager.PlaylistPath)), DeleteConfirm);
        }

        private void DeleteConfirm()
        {
            CurrentParentManager?.Parent?.DeleteChildManager(CurrentParentManager, true);
            BackButtonClicked();
        }

        #endregion

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory, bool viewControllerActivated)
        {
            if (rootTransform == null)
            {
                return;
            }

            if (levelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                rootTransform.gameObject.SetActive(true);
                SetupDimensions();
                if (viewControllerActivated)
                {
                    SetupList(CurrentParentManager, false);
                }
            }
            else
            {
                rootTransform.gameObject.SetActive(false);
            }
        }

        private void LevelFilteringNavigationController_ShowPacksInChildController_AllPacksViewSelectedEvent()
        {
            SetupList(null, false);
            folderMode = FolderMode.AllPacks;
        }

        public void Refresh()
        {
            if (!rootTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            if (folderMode == FolderMode.AllPacks)
            {
                var annotatedBeatmapLevelCollections = beatmapLevelsModel._customLevelsRepository.beatmapLevelPacks.Concat(PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks()).ToArray();
                var indexToSelect = Array.FindIndex(annotatedBeatmapLevelCollections, (pack) => pack.packID == annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack.packID);
                if (indexToSelect != -1)
                {
                    annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                }
            }
            else if (folderMode == FolderMode.Playlists)
            {
                var annotatedBeatmapLevelCollections = PlaylistLibUtils.TryGetAllPlaylistsAsLevelPacks();
                var indexToSelect = Array.FindIndex(annotatedBeatmapLevelCollections, (pack) => pack.packID == annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack.packID);
                if (indexToSelect != -1)
                {
                    annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                }
            }
            else if (folderMode == FolderMode.Folders)
            {
                var annotatedBeatmapLevelCollections = CurrentParentManager.GetAllPlaylists(false).Select(p => p.PlaylistLevelPack).ToArray();
                var indexToSelect = Array.FindIndex(annotatedBeatmapLevelCollections, (pack) => pack.packID == annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack.packID);
                if (indexToSelect != -1)
                {
                    annotatedBeatmapLevelCollectionsViewController.SetData(annotatedBeatmapLevelCollections, indexToSelect, false);
                }
                SetupList(CurrentParentManager, false);
            }
        }

        [UIValue("folder-text")]
        private string FolderText
        {
            get
            {
                if (CurrentParentManager == null || !Directory.Exists(CurrentParentManager.PlaylistPath))
                {
                    return "";
                }
                else
                {
                    var folderName = Path.GetFileName(CurrentParentManager.PlaylistPath);
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
            get => customListTableData != null && tableCells.Count > 4;
        }

        [UIValue("right-button-enabled")]
        private bool RightButtonEnabled
        {
            get => customListTableData != null && tableCells.Count > 4;
        }

        #region TableView DataSource

        private const string kReuseIdentifier = "PlaylistFolderCell";

        private FolderCell GetCell()
        {
            var tableCell = customListTableData.tableView.DequeueReusableCellForIdentifier(kReuseIdentifier);
            FolderCell? folderCell = null;

            if (tableCell == null)
            {
                tableCell = customListTableData.GetBoxTableCell();
                tableCell.reuseIdentifier = kReuseIdentifier;
                folderCell = tableCell.gameObject.AddComponent<FolderCell>();
            }

            return folderCell ? folderCell : tableCell.GetComponent<FolderCell>();
        }

        public float CellSize() => 15;

        public int NumberOfCells() => tableCells.Count;

        public TableCell CellForIdx(TableView tableView, int idx) => GetCell().PopulateCell(tableCells[idx].icon, tableCells[idx].text);

        #endregion
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
