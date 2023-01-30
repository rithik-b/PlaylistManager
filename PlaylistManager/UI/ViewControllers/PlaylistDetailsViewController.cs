using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistDetailsViewController : NotifiableBase, IInitializable, IDisposable, ILevelCollectionUpdater, INotifyPropertyChanged
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly ImageSelectionModalController imageSelectionModalController;
        private readonly PopupModalsController popupModalsController;

        private bool parsed;
        private Playlist? selectedPlaylist;
        private BeatSaberPlaylistsLib.PlaylistManager? parentManager;

        [UIComponent("modal")] 
        private readonly RectTransform modalTransform = null!;

        private Vector3 modalPosition;

        [UIComponent("name-setting")]
        private readonly RectTransform nameSettingTransform = null!;

        [UIComponent("author-setting")]
        private readonly RectTransform authorSettingTransform = null!;

        [UIComponent("playlist-cover")]
        private readonly ClickableImage playlistCoverView = null!;

        [UIComponent("text-page")]
        private readonly TextPageScrollView descriptionTextPage = null!;

        [UIParams]
        private readonly BSMLParserParams parserParams = null!;

        public PlaylistDetailsViewController(LevelPackDetailViewController levelPackDetailViewController, ImageSelectionModalController imageSelectionModalController,
            PopupModalsController popupModalsController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
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

            if (selectedPlaylist != null)
            {
                selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistDetailsView.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
                modalPosition = modalTransform.position;
            }
            modalTransform.position = modalPosition;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            parsed = true;

            var nameKeyboardModal = nameSettingTransform.Find("BSMLModalKeyboard").GetComponent<ModalView>();
            Accessors.AnimateCanvasAccessor(ref nameKeyboardModal) = false;

            var authorKeyboardModal = authorSettingTransform.Find("BSMLModalKeyboard").GetComponent<ModalView>();
            Accessors.AnimateCanvasAccessor(ref authorKeyboardModal) = false;
        }

        internal void ShowDetails()
        {
            Parse();
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");

            // Update values
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(NameHint));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(AuthorHint));
            
            UpdateReadOnly();
            NotifyPropertyChanged(nameof(PlaylistAllowDuplicates));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            
            playlistCoverView.sprite = selectedPlaylist!.Sprite;
            descriptionTextPage.ScrollTo(0, true);
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
            get => selectedPlaylist == null ? " " : (selectedPlaylist as IPlaylist).packName;
            set
            {
                if (selectedPlaylist == null || parentManager == null)
                    return;
                
                selectedPlaylist.Title = value;
                if (!selectedPlaylist.HasCover)
                {
                    selectedPlaylist.SpriteLoaded += SelectedPlaylist_SpriteLoaded;
                    selectedPlaylist.RaiseCoverImageChangedForDefaultCover();
                }
                parentManager.StorePlaylist(selectedPlaylist);
                Events.RaisePlaylistRenamed(selectedPlaylist, parentManager);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(NameHint));
            }
        }

        [UIValue("name-hint")]
        private string NameHint => string.IsNullOrWhiteSpace(PlaylistName) ? " " : PlaylistName;

        [UIValue("playlist-author")]
        private string PlaylistAuthor
        {
            get => selectedPlaylist?.Author ?? " ";
            set
            {
                if (selectedPlaylist == null || parentManager == null)
                    return;
                
                selectedPlaylist.Author = value;
                parentManager.StorePlaylist(selectedPlaylist);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(AuthorHint));
            }
        }

        [UIValue("author-hint")]
        private string AuthorHint => string.IsNullOrWhiteSpace(PlaylistAuthor) ? " " : PlaylistAuthor;

        [UIValue("playlist-description")]
        private string PlaylistDescription => selectedPlaylist?.Description ?? "";

        #endregion

        #region Read Only

        // Methods

        [UIAction("read-only-toggled")]
        private void ReadOnlyToggled(bool playlistReadOnly)
        {
            if (playlistReadOnly)
            {
                PlaylistReadOnly = true;
            }
            else if (playlistReadOnly != PlaylistReadOnly)
            {
                popupModalsController.ShowYesNoModal(modalTransform, "To turn off read only, this playlist will be cloned and writing will be enabled on the clone. Proceed?", ClonePlaylist, noButtonPressedCallback: UpdateReadOnly, animateParentCanvas: false);
            }
        }

        private void ClonePlaylist()
        {
            if (selectedPlaylist == null || parentManager == null)
            {
                return;
            }
            
            var playlistPath = Path.Combine(parentManager.PlaylistPath, $"{selectedPlaylist.Filename}.{selectedPlaylist.SuggestedExtension}");
            if (File.Exists(playlistPath))
            {
                var clonedPlaylist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(File.OpenRead(playlistPath))!;
                clonedPlaylist.ReadOnly = false;
                parentManager.StorePlaylist(clonedPlaylist);
                BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.RequestRefresh("PlaylistManager (plugin)");
                popupModalsController.ShowOkModal(modalTransform, "Playlist Cloned!", null, animateParentCanvas: false);
            }
            else
            {
                popupModalsController.ShowOkModal(modalTransform, "An error occured while trying to clone the playlist. Please try again later.", null, animateParentCanvas: false);
            }
            UpdateReadOnly();
        }

        private void UpdateReadOnly()
        {
            NotifyPropertyChanged(nameof(PlaylistReadOnly));
            NotifyPropertyChanged(nameof(ReadOnlyVisible));
            NotifyPropertyChanged(nameof(Editable));
            NotifyPropertyChanged(nameof(CoverHint));
        }

        // Values

        [UIValue("playlist-read-only")]
        private bool PlaylistReadOnly
        {
            get => selectedPlaylist is {ReadOnly: true};
            set
            {
                if (selectedPlaylist == null || parentManager == null)
                {
                    return;
                }
                
                selectedPlaylist.ReadOnly = value;
                parentManager.StorePlaylist((IPlaylist)selectedPlaylist);
                UpdateReadOnly();
            }
        }

        [UIValue("read-only-visible")]
        private bool ReadOnlyVisible => PlaylistReadOnly;

        [UIValue("editable")]
        private bool Editable => !PlaylistReadOnly;

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

        private void DeleteDuplicates() => PlaylistAllowDuplicates = false;

        private void DontDeleteDuplicates() => PlaylistAllowDuplicates = true;

        // Values

        [UIValue("playlist-allow-duplicates")]
        private bool PlaylistAllowDuplicates
        {
            get => selectedPlaylist is {AllowDuplicates: true};
            set
            {
                if (selectedPlaylist == null || parentManager == null)
                {
                    return;
                }
                
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

                parentManager.StorePlaylist(selectedPlaylist);
                NotifyPropertyChanged();
            }

        }

        #endregion

        #region Playlist Cover

        [UIAction("playlist-cover-clicked")]
        private void OpenImageSelectionModal()
        {
            if (!PlaylistReadOnly)
            {
                imageSelectionModalController.ShowModal(selectedPlaylist!);
            }
        }

        private void ImageSelectionModalController_ImageSelectedEvent(byte[]? imageBytes)
        {
            selectedPlaylist!.SpriteLoaded += SelectedPlaylist_SpriteLoaded;
            try
            {
                selectedPlaylist.SetCover(imageBytes!);
                _ = selectedPlaylist.Sprite;
                parentManager!.StorePlaylist(selectedPlaylist);
            }
            catch (Exception e)
            {
                popupModalsController.ShowOkModal(modalTransform, "There was an error loading this image. Check logs for more details.", null, animateParentCanvas: false);
                Plugin.Log.Critical(e.Message);
            }
        }

        private void SelectedPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is IDeferredSpriteLoad deferredSpriteLoad)
            {
                playlistCoverView.sprite = deferredSpriteLoad.Sprite;
                deferredSpriteLoad.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }
        }

        [UIValue("cover-hint")]
        private string CoverHint => PlaylistReadOnly ? "Cover Image" : "Set Cover";

        #endregion

        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager? parentManager)
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
