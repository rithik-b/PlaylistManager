using IPA.Utilities;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    using System.Linq;

    class PlaylistSegmentedControl : IInitializable, IDisposable
    {
        private SelectLevelCategoryViewController selectLevelCategoryViewController;
        public static readonly FieldAccessor<SelectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategoryInfo[]>.Accessor AllLevelCategoryInfosAccessor
            = FieldAccessor<SelectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategoryInfo[]>.GetAccessor("_allLevelCategoryInfos");
        private SelectLevelCategoryViewController.LevelCategoryInfo[] oldLevelCategoryInfos;

        PlaylistSegmentedControl(SelectLevelCategoryViewController selectLevelCategoryViewController)
        {
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
        }

        public void Initialize()
        {
            oldLevelCategoryInfos = AllLevelCategoryInfosAccessor(ref selectLevelCategoryViewController);
            SelectLevelCategoryViewController.LevelCategoryInfo playlistLevelCategoryInfo = new SelectLevelCategoryViewController.LevelCategoryInfo();
            playlistLevelCategoryInfo.levelCategory = SelectLevelCategoryViewController.LevelCategory.CustomSongs;
            playlistLevelCategoryInfo.localizedKey = "Playlists";
            playlistLevelCategoryInfo.categoryIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Playlist.png");
            FieldAccessor<SelectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategoryInfo[]>.Set(ref selectLevelCategoryViewController, "_allLevelCategoryInfos", oldLevelCategoryInfos.Append(playlistLevelCategoryInfo).ToArray());
        }

        public void Dispose()
        {
            FieldAccessor<SelectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategoryInfo[]>.Set(ref selectLevelCategoryViewController, "_allLevelCategoryInfos", oldLevelCategoryInfos);
        }
    }
}
