using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.Interfaces;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace PlaylistManager.UI
{
    public class PlaylistDetailsViewController : ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;

        private Vector3 modalPosition;
        private bool parsed;
        private Playlist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("modal")]
        private readonly ModalView modal;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIValue("playlist-name")]
        private string PlaylistName
        {
            get => selectedPlaylist == null ? "" : selectedPlaylist.Title;
            set
            {
                selectedPlaylist.Title = value;
                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
            }
        }

        [UIValue("playlist-author")]
        private string PlaylistAuthor
        {
            get => selectedPlaylist == null ? "" : selectedPlaylist.Author;
            set
            {
                selectedPlaylist.Author = value;
                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAuthor)));
            }
        }

        [UIValue("playlist-description")]
        private string PlaylistDescription
        {
            get => selectedPlaylist == null ? "" : selectedPlaylist.Description;
            set
            {
                selectedPlaylist.Description = value;
                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistDescription)));
            }
        }

        public PlaylistDetailsViewController(LevelPackDetailViewController levelPackDetailViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistDetailsView.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
                modalPosition = modalTransform.position;
                parsed = true;
            }
            modalTransform.position = modalPosition;
        }

        internal void ShowDetails()
        {
            Parse();
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
        }

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (annotatedBeatmapLevelCollection is Playlist selectedPlaylist)
            {
                this.selectedPlaylist = selectedPlaylist;
                this.parentManager = parentManager;
            }
            else
            {
                selectedPlaylist = null;
                parentManager = null;
            }
        }
    }
}
