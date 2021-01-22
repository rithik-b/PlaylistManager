using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using PlaylistManager.Interfaces;
using System.Reflection;
using Zenject;
using UnityEngine;
using BeatSaberMarkupLanguage.Components;
using BeatSaberPlaylistsLib.Types;

namespace PlaylistManager.UI
{
    class ButtonViewController : IInitializable, IPreviewBeatmapLevelUpdater
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private AddPlaylistController addPlaylistController;
        private RemoveFromPlaylistController removeFromPlaylistController;

        [UIComponent("add-remove-button")]
        private Transform addRemoveButtonTransform;

        internal enum ButtonState
        {
            AddButton,
            RemoveButton,
            Inactive
        }

        private ButtonState _buttonState;

        internal ButtonState buttonState
        {
            get
            {
                return _buttonState;
            }
            set
            {
                _buttonState = value;
                addRemoveButtonTransform.gameObject.SetActive(true);
                switch (_buttonState)
                {
                    case ButtonState.AddButton:
                        addRemoveButtonTransform.GetComponent<ButtonIconImage>().SetIcon("PlaylistManager.Icons.AddToPlaylist.png");
                        break;
                    case ButtonState.RemoveButton:
                        addRemoveButtonTransform.GetComponent<ButtonIconImage>().SetIcon("PlaylistManager.Icons.RemoveFromPlaylist.png");
                        break;
                    default:
                        addRemoveButtonTransform.gameObject.SetActive(false);
                        break;
                }
            }
        }

        ButtonViewController(StandardLevelDetailViewController standardLevelDetailViewController, AddPlaylistController addPlaylistController, RemoveFromPlaylistController removeFromPlaylistController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.addPlaylistController = addPlaylistController;
            this.removeFromPlaylistController = removeFromPlaylistController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.ButtonView.bsml"), standardLevelDetailViewController.transform.Find("LevelDetail").gameObject, this);
            addRemoveButtonTransform.localScale *= 0.7f;
        }

        [UIAction("button-click")]
        internal void OpenModal()
        {
            switch (_buttonState)
            {
                case ButtonState.AddButton:
                    if (!addPlaylistController.parsed)
                    {
                        addPlaylistController.Parse();
                        addPlaylistController.parsed = true;
                    }
                    addPlaylistController.ShowPlaylists();
                    break;
                case ButtonState.RemoveButton:
                    if (!removeFromPlaylistController.parsed)
                    {
                        removeFromPlaylistController.Parse();
                        removeFromPlaylistController.parsed = true;
                    }
                    removeFromPlaylistController.DisplayWarning();
                    break;
            }
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if (beatmapLevel.levelID.EndsWith(" WIP"))
            {
                buttonState = ButtonState.Inactive;
            }
            else if (beatmapLevel is IPlaylistSong)
            {
                buttonState = ButtonState.RemoveButton;
            }
            else
            {
                buttonState = ButtonState.AddButton;
            }
        }
    }
}
