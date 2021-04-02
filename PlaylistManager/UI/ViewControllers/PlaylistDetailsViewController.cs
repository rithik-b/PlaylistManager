using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Interfaces;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistDetailsViewController : IInitializable, IDisposable, ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private LevelCollectionNavigationController levelCollectionNavigationController;
        private readonly ImageSelectionModalController imageSelectionModalController;
        private readonly PopupModalsController popupModalsController;

        private bool parsed;
        private Playlist selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager parentManager;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIComponent("name-setting")]
        private RectTransform nameSettingTransform;

        [UIComponent("author-setting")]
        private RectTransform authorSettingTransform;

        [UIComponent("playlist-cover")]
        private readonly ClickableImage playlistCoverView;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public PlaylistDetailsViewController(LevelPackDetailViewController levelPackDetailViewController, LevelCollectionNavigationController levelCollectionNavigationController, ImageSelectionModalController imageSelectionModalController, PopupModalsController popupModalsController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.levelCollectionNavigationController = levelCollectionNavigationController;
            this.imageSelectionModalController = imageSelectionModalController;
            this.popupModalsController = popupModalsController;
            parsed = false;
        }

        public void Initialize()
        {
            imageSelectionModalController.ImageSelectedEvent += ImageSelectionModalController_ImageSelectedEvent;
        }

        public void Dispose()
        {
            imageSelectionModalController.ImageSelectedEvent -= ImageSelectionModalController_ImageSelectedEvent;

            if (this.selectedPlaylist != null)
            {
                this.selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistDetailsView.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
                modalPosition = modalTransform.position;

                ModalView nameKeyboardModal = nameSettingTransform.Find("BSMLModalKeyboard").GetComponent<ModalView>();
                FieldAccessor<ModalView, bool>.Set(ref nameKeyboardModal, "_animateParentCanvas", false);

                ModalView authorKeyboardModal = authorSettingTransform.Find("BSMLModalKeyboard").GetComponent<ModalView>();
                FieldAccessor<ModalView, bool>.Set(ref authorKeyboardModal, "_animateParentCanvas", false);

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameHint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAuthor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AuthorHint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAllowDuplicates)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistDescription)));
            playlistCoverView.sprite = selectedPlaylist.Sprite;
        }

        #region Name, Author, Description

        // Methods

        [UIAction("string-formatter")]
        private string StringFormatter(string inputString)
        {
            if (inputString.Length > 15)
            {
                return inputString.Substring(0, 15) + "...";
            }
            return inputString;
        }

        // Values

        [UIValue("playlist-name")]
        private string PlaylistName
        {
            get => selectedPlaylist == null || selectedPlaylist.Title == null ? "" : selectedPlaylist.Title;
            set
            {
                selectedPlaylist.Title = value;
                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
            }
        }

        [UIValue("name-hint")]
        private string NameHint
        {
            get => PlaylistName.Length > 15 ? PlaylistName : "";
        }

        [UIValue("playlist-author")]
        private string PlaylistAuthor
        {
            get => selectedPlaylist == null || selectedPlaylist.Author == null ? "" : selectedPlaylist.Author;
            set
            {
                selectedPlaylist.Author = value;
                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAuthor)));
            }
        }

        [UIValue("author-hint")]
        private string AuthorHint
        {
            get => PlaylistAuthor.Length > 15 ? PlaylistAuthor : "";
        }

        [UIValue("playlist-description")]
        private string PlaylistDescription
        {
            get => selectedPlaylist == null || selectedPlaylist.Description == null ? "" : selectedPlaylist.Description;
        }

        #endregion

        #region Allow Duplicates

        // Methods

        [UIAction("duplicates-toggled")]
        private void DuplicatesToggled(bool playlistAllowDuplicates)
        {
            if (playlistAllowDuplicates)
            {
                PlaylistAllowDuplicates = true;
            }
            else if (PlaylistAllowDuplicates != playlistAllowDuplicates)
            {
                popupModalsController.ShowYesNoModal(modalTransform, "Are you sure you want to turn off duplicates for this playlist? This will also delete all duplicate songs from this playlist.", DeleteDuplicates, noButtonPressedCallback: DontDeleteDuplicates, animateParentCanvas: false);
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

        // Values

        [UIValue("playlist-allow-duplicates")]
        private bool PlaylistAllowDuplicates
        {
            get => selectedPlaylist == null ? false : selectedPlaylist.AllowDuplicates;
            set
            {
                selectedPlaylist.AllowDuplicates = value;

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

        #endregion

        #region Playlist Cover

        [UIAction("playlist-cover-clicked")]
        private void OpenImageSelectionModal()
        {
            imageSelectionModalController.ShowModal();
        }

        private void ImageSelectionModalController_ImageSelectedEvent(byte[] imageBytes)
        {
            selectedPlaylist.SpriteLoaded += SelectedPlaylist_SpriteLoaded;
            try
            {
                selectedPlaylist.SetCover(imageBytes);
                _ = selectedPlaylist.Sprite;
                parentManager.StorePlaylist((BeatSaberPlaylistsLib.Types.IPlaylist)selectedPlaylist);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(modalTransform, "There was an error loading this image. Check logs for more details.", null, animateParentCanvas: false);
                Plugin.Log.Critical(e.Message);
            }
        }

        private void SelectedPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            playlistCoverView.sprite = selectedPlaylist.Sprite;
            levelPackDetailViewController.SetData((IBeatmapLevelPack)selectedPlaylist);
            levelPackDetailViewController.ShowContent(LevelPackDetailViewController.ContentType.Owned);
            FieldAccessor<LevelCollectionNavigationController, IBeatmapLevelPack>.Set(ref levelCollectionNavigationController, "_levelPack", (IBeatmapLevelPack)selectedPlaylist);
            selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
        }

        #endregion

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (this.selectedPlaylist != null)
            {
                this.selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }

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
