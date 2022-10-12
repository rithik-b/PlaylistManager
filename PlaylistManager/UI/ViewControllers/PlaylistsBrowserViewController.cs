using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.Utilities;
using UnityEngine;

namespace PlaylistManager.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\PlaylistsBrowserView.bsml")]
    [ViewDefinition("PlaylistManager.UI.Views.PlaylistsBrowserView.bsml")]
    internal class PlaylistsBrowserViewController : NotifiableBase
    {
        public BeatSaberPlaylistsLib.PlaylistManager ParentManager { get; private set; } =
            BeatSaberPlaylistsLib.PlaylistManager.DefaultManager;
        public IReadOnlyList<BeatSaberPlaylistsLib.PlaylistManager>? ChildManagers { get; private set; } 
        public IReadOnlyList<IPlaylist>? ChildPlaylists { get; private set; }

        private readonly Sprite folderIcon;
        private Action<IPlaylist>? onPlaylistSelected;
        private bool parsed;
        
        [UIComponent("root")]
        private readonly RectTransform rootTransform = null!;
        
        [UIComponent("heading")] 
        private ImageView headingImageView = null!;
        
        [UIComponent("folder-icon")]
        private ImageView folderIconImageView = null!;

        [UIComponent("list")]
        private readonly CustomListTableData playlistTableData = null!;

        public PlaylistsBrowserViewController()
        {
            folderIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.FolderIcon.png");
            parsed = false;
        }
        
        private void Parse(GameObject parent)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistsBrowserView.bsml"), parent, this);
            }
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            parsed = true;
            
            // Set up header
            Accessors.SkewAccessor(ref headingImageView) = 0;
            headingImageView.color0 = new Color(0.5f, 0.5f, 1, 1);
            headingImageView.color1 = new Color(1, 1, 1, 0);
            folderIconImageView.sprite = folderIcon;
        }

        #region Show Playlists
        
        public void ShowPlaylistBrowser(Transform parent, Action<IPlaylist>? onPlaylistSelected)
        {
            this.onPlaylistSelected = onPlaylistSelected;
            Parse(parent.gameObject);
            rootTransform.SetParent(parent, false);
            ShowPlaylistsForManager(BeatSaberPlaylistsLib.PlaylistManager.DefaultManager);
        }
            
        private void ShowPlaylistsForManager(BeatSaberPlaylistsLib.PlaylistManager parentManager, bool keepPosition = false)
        {
            playlistTableData.data.Clear();

            ParentManager = parentManager;
            ChildManagers = parentManager.GetChildManagers().ToList();
            ChildPlaylists = parentManager.GetAllPlaylists(false).Where(playlist => !playlist.ReadOnly).ToList();
            
            foreach (var playlistManager in ChildManagers)
            {
                playlistTableData.data.Add(new CustomListTableData.CustomCellInfo(Path.GetFileName(playlistManager.PlaylistPath), "Folder", folderIcon));
            }
            foreach (var playlist in ChildPlaylists)
            {
                if (playlist is IStagedSpriteLoad {SmallSpriteWasLoaded: false} stagedSpriteLoadPlaylist)
                {
                    stagedSpriteLoadPlaylist.SpriteLoaded -= StagedSpriteLoadPlaylist_SpriteLoaded;
                    stagedSpriteLoadPlaylist.SpriteLoaded += StagedSpriteLoadPlaylist_SpriteLoaded;
                    _ = playlist.smallCoverImage;
                }
                else
                {
                    ShowPlaylist(playlist);
                }
            }

            if (keepPosition)
            {
                playlistTableData.tableView.ReloadDataKeepingPosition();
            }
            else
            {
                playlistTableData.tableView.ReloadData();
            }

            NotifyPropertyChanged(nameof(BackActive));
            NotifyPropertyChanged(nameof(FolderText));
        }

        private void StagedSpriteLoadPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is not IStagedSpriteLoad stagedSpriteLoadPlaylist)
            {
                return;
            }
            
            if (ParentManager.GetAllPlaylists(false).Contains((IPlaylist)stagedSpriteLoadPlaylist))
            {
                ShowPlaylist((IPlaylist)stagedSpriteLoadPlaylist);
            }
            
            playlistTableData.tableView.ReloadDataKeepingPosition();
            (stagedSpriteLoadPlaylist).SpriteLoaded -= StagedSpriteLoadPlaylist_SpriteLoaded;
        }

        private void ShowPlaylist(IPlaylist playlist)
        {
            var subName = $"{playlist.beatmapLevelCollection.beatmapLevels.Count} songs";
            playlistTableData.data.Add(new CustomListTableData.CustomCellInfo(playlist.collectionName, subName, playlist.smallCoverImage));
        }

        #endregion
        
        public void Refresh()
        {
            ShowPlaylistsForManager(ParentManager, true);
        }
        
        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            playlistTableData.tableView.ClearSelection();
            // Folder Selected
            if (index < ChildManagers!.Count)
            {
                ShowPlaylistsForManager(ChildManagers[index]);
            }
            else
            {
                index -= ChildManagers.Count;
                var selectedPlaylist = ChildPlaylists![index];
                onPlaylistSelected?.Invoke(selectedPlaylist);
            }
        }

        [UIAction("back-button-pressed")]
        private void BackButtonPressed()
        {
            ShowPlaylistsForManager(ParentManager.Parent!);
        }

        [UIValue("folder-text")]
        private string FolderText => Path.GetFileName(ParentManager.PlaylistPath);

        [UIValue("back-active")]
        private bool BackActive => ParentManager is {Parent: { }};
    }
}