using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    class FoldersViewController : IInitializable, ILevelCategoryUpdater, INotifyPropertyChanged
    {
        private readonly HMUI.Screen bottomScreen;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly PopupModalsController popupModalsController;
        private readonly Sprite customSongsCover;
        private BeatmapLevelsModel beatmapLevelsModel;

        public event PropertyChangedEventHandler PropertyChanged;
        private BeatSaberPlaylistsLib.PlaylistManager currentParentManager;
        private List<BeatSaberPlaylistsLib.PlaylistManager> currentManagers;
        private string _folderText = "";

        public static readonly FieldAccessor<HierarchyManager, ScreenSystem>.Accessor ScreenSystemAccessor = FieldAccessor<HierarchyManager, ScreenSystem>.GetAccessor("_screenSystem");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.Accessor AnnotatedBeatmapLevelCollectionsTableViewAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.GetAccessor("_annotatedBeatmapLevelCollectionsTableView");
        public static readonly FieldAccessor<CustomLevelLoader, Sprite>.Accessor DefaultPackCoverAccessor = FieldAccessor<CustomLevelLoader, Sprite>.GetAccessor("_defaultPackCover");
        public static readonly FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.Accessor CustomLevelPackCollectionAccessor = FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.GetAccessor("_customLevelPackCollection");

        [UIComponent("root")]
        private RectTransform rootTransform;

        [UIComponent("back-rect")]
        private RectTransform backTransform;

        [UIComponent("folder-list")]
        public CustomListTableData customListTableData = null;

        FoldersViewController(HierarchyManager hierarchyManager, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionNavigationController levelCollectionNavigationController, PopupModalsController popupModalsController, CustomLevelLoader customLevelLoader, BeatmapLevelsModel beatmapLevelsModel)
        {
            ScreenSystem screenSystem = ScreenSystemAccessor(ref hierarchyManager);
            bottomScreen = screenSystem.bottomScreen;

            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.popupModalsController = popupModalsController;

            customSongsCover = DefaultPackCoverAccessor(ref customLevelLoader);

            this.beatmapLevelsModel = beatmapLevelsModel;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.FoldersView.bsml"), bottomScreen.gameObject, this);
            rootTransform.gameObject.SetActive(false);
        }

        private void SetupList(BeatSaberPlaylistsLib.PlaylistManager currentParentManager = null)
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            this.currentParentManager = currentParentManager;

            if (currentParentManager == null)
            {
                CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo("Custom Songs", icon: customSongsCover);
                customListTableData.data.Add(customCellInfo);

                customCellInfo = new CustomListTableData.CustomCellInfo("Playlists", icon: BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Playlist.png"));
                customListTableData.data.Add(customCellInfo);

                backTransform.gameObject.SetActive(false);
            }
            else
            {
                currentManagers = currentParentManager.GetChildManagers().ToList();

                if (currentParentManager.GetAllPlaylists(false).Length != 0)
                {
                    currentManagers.Insert(0, currentParentManager);
                }

                foreach (var childManager in currentManagers)
                {
                    var folderName = Path.GetFileName(childManager.PlaylistPath);
                    if (childManager == currentParentManager)
                    {
                        folderName = "/";
                    }
                    CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: PlaylistLibUtils.DrawFolderIcon(folderName));
                    customListTableData.data.Add(customCellInfo);
                }

                backTransform.gameObject.SetActive(true);
                FolderText = Path.GetFileName(currentParentManager.PlaylistPath);
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }

        [UIAction("folder-select")]
        private void Select(TableView _, int row)
        {
            if (currentParentManager == null) // If we are at root
            {
                if (row == 0)
                {
                    IBeatmapLevelPack[] beatmapLevelPacks = CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks;
                    annotatedBeatmapLevelCollectionsViewController.SetData(beatmapLevelPacks, 0, false);
                    annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController), beatmapLevelPacks[0]);
                }
                else if (row == 1)
                {
                    SetupList(currentParentManager: PlaylistLibUtils.playlistManager);
                }
            }
            else
            {
                if (currentManagers[row] != currentParentManager)
                {
                    SetupList(currentParentManager: currentManagers[row]);
                }
                else if (currentManagers[row].GetAllPlaylists(false).Length != 0) // Only set data if not empty
                {
                    IBeatmapLevelPack[] beatmapLevelPacks = currentManagers[row].GetAllPlaylists(false);
                    annotatedBeatmapLevelCollectionsViewController.SetData(beatmapLevelPacks, 0, false);
                    annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController), beatmapLevelPacks[0]);
                }
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

        [UIAction("create-folder")]
        private void CreateFolder()
        {
            popupModalsController.ShowKeyboard(levelCollectionNavigationController.transform, KeyboardEnter);
        }

        private void KeyboardEnter(string folderName)
        {
            if (!string.IsNullOrEmpty(folderName))
            {
                BeatSaberPlaylistsLib.PlaylistManager childManager = currentParentManager.CreateChildManager(folderName);

                CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: PlaylistLibUtils.DrawFolderIcon(folderName));
                customListTableData.data.Add(customCellInfo);
                customListTableData.tableView.ReloadData();
                customListTableData.tableView.ClearSelection();

                currentManagers.Add(childManager);
            }
        }

        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            if (levelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                bottomScreen.gameObject.SetActive(true);
                rootTransform.gameObject.SetActive(true);
                SetupList();
            }
            else
            {
                rootTransform.gameObject.SetActive(false);
            }
        }

        [UIValue("folder-text")]
        private string FolderText
        {
            get => _folderText;
            set
            {
                _folderText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderText)));
            }
        }
    }
}
