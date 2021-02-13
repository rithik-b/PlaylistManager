using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberPlaylistsLib;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    class FoldersViewController : IInitializable, ILevelCategoryUpdater
    {
        private HMUI.Screen bottomScreen;
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        private Sprite customSongsCover;
        private BeatmapLevelsModel beatmapLevelsModel;

        private List<BeatSaberPlaylistsLib.PlaylistManager> childManagers;

        public static readonly FieldAccessor<HierarchyManager, ScreenSystem>.Accessor ScreenSystemAccessor = FieldAccessor<HierarchyManager, ScreenSystem>.GetAccessor("_screenSystem");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.Accessor AnnotatedBeatmapLevelCollectionsTableViewAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.GetAccessor("_annotatedBeatmapLevelCollectionsTableView");
        public static readonly FieldAccessor<CustomLevelLoader, Sprite>.Accessor DefaultPackCoverAccessor = FieldAccessor<CustomLevelLoader, Sprite>.GetAccessor("_defaultPackCover");
        public static readonly FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.Accessor CustomLevelPackCollectionAccessor = FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.GetAccessor("_customLevelPackCollection");

        [UIComponent("folderList")]
        public CustomListTableData customListTableData = null;

        [UIComponent("root")]
        private RectTransform rootTransform;

        FoldersViewController(HierarchyManager hierarchyManager, AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, CustomLevelLoader customLevelLoader, BeatmapLevelsModel beatmapLevelsModel)
        {
            ScreenSystem screenSystem = ScreenSystemAccessor(ref hierarchyManager);
            bottomScreen = screenSystem.bottomScreen;

            this.annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;

            customSongsCover = DefaultPackCoverAccessor(ref customLevelLoader);

            this.beatmapLevelsModel = beatmapLevelsModel;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.FoldersView.bsml"), bottomScreen.gameObject, this);
            rootTransform.gameObject.SetActive(false);
        }

        private void SetupList()
        {
            customListTableData.data.Clear();

            CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo("Custom Songs", icon: customSongsCover);
            customListTableData.data.Add(customCellInfo);

            customCellInfo = new CustomListTableData.CustomCellInfo("Playlists", icon: BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Playlist.png"));
            customListTableData.data.Add(customCellInfo);

            childManagers = PlaylistLibUtils.playlistManager.GetChildManagers().ToList();
            foreach (var childManager in childManagers)
            {
                var folderName = Path.GetDirectoryName(childManager.PlaylistPath);
                customCellInfo = new CustomListTableData.CustomCellInfo(folderName, icon: BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Playlist.png"));
                customListTableData.data.Add(customCellInfo);
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, false);
            customListTableData.tableView.SelectCellWithIdx(0);
        }

        [UIAction("folderSelect")]
        public void Select(TableView _, int row)
        {
            if (row == 0)
            {
                IBeatmapLevelPack[] beatmapLevelPacks = CustomLevelPackCollectionAccessor(ref beatmapLevelsModel).beatmapLevelPacks;
                annotatedBeatmapLevelCollectionsViewController.SetData(beatmapLevelPacks, 0, false);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController), beatmapLevelPacks[0]);
            }
            else if (row == 1)
            {
                IBeatmapLevelPack[] beatmapLevelPacks = (IBeatmapLevelPack[])PlaylistLibUtils.playlistManager.GetAllPlaylists(false);
                annotatedBeatmapLevelCollectionsViewController.SetData(beatmapLevelPacks, 0, false);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController), beatmapLevelPacks[0]);
            }
            else
            {
                IBeatmapLevelPack[] beatmapLevelPacks = childManagers[row - 2].GetAllPlaylists(true);
                annotatedBeatmapLevelCollectionsViewController.SetData(beatmapLevelPacks, 0, false);
                annotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsViewController), beatmapLevelPacks[0]);
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
    }
}
