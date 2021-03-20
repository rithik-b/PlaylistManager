using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace PlaylistManager.UI
{
    public class PlaylistDetailsViewController : ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;

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

        [UIValue("playlist-allow-duplicates")]
        private bool PlaylistAllowDuplicates
        {
            get => selectedPlaylist == null ? false : selectedPlaylist.AllowDuplicates;
            set
            {
                selectedPlaylist.AllowDuplicates = value;

                if (selectedPlaylist.CustomData == null)
                {
                    selectedPlaylist.CustomData = new Dictionary<string, object>();
                }
                selectedPlaylist.CustomData["AllowDuplicates"] = value;

                if (!value)
                {
                    if (selectedPlaylist is BlistPlaylist blistPlaylist)
                    {
                        blistPlaylist.RemoveDuplicates();
                    }
                    else if (selectedPlaylist is LegacyPlaylist legacyPlaylist)
                    {
                        legacyPlaylist.RemoveDuplicates();
                    }
                }

                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAllowDuplicates)));
            }

        }

        [UIValue("playlist-description")]
        private string PlaylistDescription
        {
            get => selectedPlaylist == null ? "" : selectedPlaylist.Description;
        }

        public PlaylistDetailsViewController(LevelPackDetailViewController levelPackDetailViewController, PopupModalsController popupModalsController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;
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

            // Update values
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAuthor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAllowDuplicates)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistDescription)));
        }

        [UIAction("duplicates-toggled")]
        private void DuplicatesToggled(bool playlistAllowDuplicates)
        {
            if (playlistAllowDuplicates)
            {
                PlaylistAllowDuplicates = true;
            }
            else
            {
                popupModalsController.ShowYesNoModal(modalTransform, "Are you sure you want to turn off duplicates for this playlist? This will also delete all duplicate songs from this playlist.", DeleteDuplicates, noButtonPressedCallback: DontDeleteDuplicates);
            }
        }

        private void DeleteDuplicates()
        {
            PlaylistAllowDuplicates = false;
        }

        private void DontDeleteDuplicates()
        {
            PlaylistAllowDuplicates = true;
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
                this.selectedPlaylist = null;
                this.parentManager = null;
            }
        }
    }
}
