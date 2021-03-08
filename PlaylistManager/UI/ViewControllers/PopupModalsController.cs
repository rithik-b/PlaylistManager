﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using PlaylistManager.Interfaces;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace PlaylistManager.UI
{
    public class PopupModalsController : IPlaylistManagerModal, INotifyPropertyChanged
    {
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private bool parsed;
        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void ButtonPressed();
        private ButtonPressed yesButtonPressed;
        private ButtonPressed okButtonPressed;

        public delegate void KeyboardPressed(string keyboardText);
        private KeyboardPressed keyboardPressed;

        private string _yesNoText = "";
        private string _yesButtonText = "Yes";
        private string _noButtonText = "No";

        private string _okText = "";
        private string _okButtonText = "Ok";

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("yes-no-modal")]
        private readonly RectTransform yesNoModalTransform;

        private Vector3 yesNoModalPosition;

        [UIComponent("ok-modal")]
        private readonly RectTransform okModalTransform;

        private Vector3 okModalPosition;

        [UIComponent("keyboard")]
        private readonly RectTransform keyboardTransform;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public PopupModalsController(LevelCollectionNavigationController levelCollectionNavigationController)
        {
            this.levelCollectionNavigationController = levelCollectionNavigationController;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PopupModals.bsml"), levelCollectionNavigationController.gameObject, this);
                yesNoModalPosition = yesNoModalTransform.position;
                okModalPosition = okModalTransform.position;
                parsed = true;
            }
        }

        internal void ShowYesNoModal(Transform parent, string text, ButtonPressed buttonPressedCallback, string yesButtonText = "Yes", string noButtonText = "No")
        {
            Parse();
            yesNoModalTransform.parent = parent;
            YesNoText = text;
            YesButtonText = yesButtonText;
            NoButtonText = noButtonText;
            yesButtonPressed = buttonPressedCallback;
            parserParams.EmitEvent("open-yes-no");
        }

        [UIAction("yes-button-pressed")]
        private void YesButtonPressed()
        {
            yesButtonPressed?.Invoke();
            yesButtonPressed = null;
        }

        internal void ShowOkModal(Transform parent, string text, ButtonPressed buttonPressedCallback, string okButtonText = "Ok")
        {
            Parse();
            okModalTransform.parent = parent;
            OkText = text;
            OkButtonText = okButtonText;
            okButtonPressed = buttonPressedCallback;
            parserParams.EmitEvent("ok-button-pressed");
        }

        [UIAction("ok-button-pressed")]
        private void OkButtonPressed()
        {
            okButtonPressed?.Invoke();
            okButtonPressed = null;
        }

        internal void ShowKeyboard(Transform parent, KeyboardPressed keyboardPressedCallback)
        {
            Parse();
            keyboardTransform.parent = parent;
            keyboardPressed = keyboardPressedCallback;
            parserParams.EmitEvent("open-keyboard");
        }

        [UIAction("keyboard-enter")]
        private void KeyboardEnter(string keyboardText)
        {
            keyboardPressed?.Invoke(keyboardText);
            keyboardPressed = null;
        }

        public void ParentControllerDeactivated()
        {
            if (parsed && rootTransform != null && yesNoModalTransform != null && okModalTransform != null && keyboardTransform != null)
            {
                yesNoModalTransform.transform.SetParent(rootTransform);
                yesNoModalTransform.position = yesNoModalPosition;

                okModalTransform.transform.SetParent(rootTransform);
                okModalTransform.position = okModalPosition;

                keyboardTransform.transform.SetParent(rootTransform);
            }
        }

        [UIValue("yes-no-text")]
        private string YesNoText
        {
            get => _yesNoText;
            set
            {
                _yesNoText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(YesNoText)));
            }
        }

        [UIValue("yes-button-text")]
        private string YesButtonText
        {
            get => _yesButtonText;
            set
            {
                _yesButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(YesButtonText)));
            }
        }

        [UIValue("no-button-text")]
        private string NoButtonText
        {
            get => _noButtonText;
            set
            {
                _noButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NoButtonText)));
            }
        }

        [UIValue("ok-text")]
        private string OkText
        {
            get => _okText;
            set
            {
                _okText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkText)));
            }
        }

        [UIValue("ok-button-text")]
        private string OkButtonText
        {
            get => _okButtonText;
            set
            {
                _okButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkButtonText)));
            }
        }
    }
}