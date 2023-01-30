using BeatSaberPlaylistsLib.Types;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;

namespace PlaylistManager.UI
{
    internal class CoverImageUpdater : ILevelCollectionUpdater
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private LevelCollectionNavigationController levelCollectionNavigationController;

        private Playlist? selectedPlaylist;

        public CoverImageUpdater(LevelPackDetailViewController levelPackDetailViewController, LevelCollectionNavigationController levelCollectionNavigationController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.levelCollectionNavigationController = levelCollectionNavigationController;
        }

        private void SelectedPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is not Playlist selectedPlaylist) 
                return;
            
            levelPackDetailViewController.SetData(selectedPlaylist);
            levelPackDetailViewController.ShowContent(LevelPackDetailViewController.ContentType.Owned);
            Accessors.LevelPackAccessor(ref levelCollectionNavigationController) = selectedPlaylist;
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager? parentManager)
        {
            if (this.selectedPlaylist != null)
            {
                this.selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }

            if (annotatedBeatmapLevelCollection is Playlist selectedPlaylist)
            {
                this.selectedPlaylist = selectedPlaylist;
                selectedPlaylist.SpriteLoaded += SelectedPlaylist_SpriteLoaded;
            }
            else
            {
                this.selectedPlaylist = null;
            }
        }
    }
}
