using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using PlaylistManager.Interfaces;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace PlaylistManager.UI
{
    public class PopupModalsController : INotifyPropertyChanged, IStackedModalView
    {
        private readonly LevelCollectionNavigationController levelCollectionNavigationController;
        private bool parsed;
        private bool animateDismiss;
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action ModalDismissedEvent;

        public delegate void ButtonPressed();
        private ButtonPressed yesButtonPressed;
        private ButtonPressed noButtonPressed;
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

        [UIComponent("yes-no-modal")]
        private readonly ModalView yesNoModalView;

        private Vector3 yesNoModalPosition;

        [UIComponent("ok-modal")]
        private readonly RectTransform okModalTransform;

        [UIComponent("ok-modal")]
        private readonly ModalView okModalView;

        private Vector3 okModalPosition;

        [UIComponent("keyboard")]
        private readonly RectTransform keyboardTransform;

        [UIComponent("keyboard")]
        private readonly ModalView keyboardModalView;

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

        #region Yes/No Modal

        // Methods

        internal void ShowYesNoModal(Transform parent, string text, ButtonPressed yesButtonPressedCallback, string yesButtonText = "Yes", string noButtonText = "No", ButtonPressed noButtonPressedCallback = null, bool animateDismiss = true)
        {
            Parse();
            yesNoModalTransform.position = yesNoModalPosition;
            yesNoModalTransform.transform.SetParent(parent);
            YesNoText = text;
            YesButtonText = yesButtonText;
            NoButtonText = noButtonText;
            yesButtonPressed = yesButtonPressedCallback;
            noButtonPressed = noButtonPressedCallback;
            this.animateDismiss = animateDismiss;
            parserParams.EmitEvent("close-yes-no");
            parserParams.EmitEvent("open-yes-no");
        }

        [UIAction("yes-button-pressed")]
        private void YesButtonPressed()
        {
            yesButtonPressed?.Invoke();
            Action modalDismissed = animateDismiss ? null : ModalDismissedEvent;
            yesNoModalView.Hide(animateDismiss, modalDismissed);
            yesButtonPressed = null;
            yesNoModalTransform.transform.SetParent(rootTransform);
        }

        [UIAction("no-button-pressed")]
        private void NoButtonPressed()
        {
            noButtonPressed?.Invoke();
            Action modalDismissed = animateDismiss ? null : ModalDismissedEvent;
            yesNoModalView.Hide(animateDismiss, modalDismissed);
            noButtonPressed = null;
            yesNoModalTransform.transform.SetParent(rootTransform);
        }

        // Values

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

        #endregion

        #region Ok Modal

        // Methods

        internal void ShowOkModal(Transform parent, string text, ButtonPressed buttonPressedCallback, string okButtonText = "Ok", bool animateDismiss = true)
        {
            Parse();
            okModalTransform.position = okModalPosition;
            okModalTransform.transform.SetParent(parent);
            OkText = text;
            OkButtonText = okButtonText;
            okButtonPressed = buttonPressedCallback;
            this.animateDismiss = animateDismiss;
            parserParams.EmitEvent("close-ok");
            parserParams.EmitEvent("open-ok");
        }

        [UIAction("ok-button-pressed")]
        private void OkButtonPressed()
        {
            okButtonPressed?.Invoke();
            Action modalDismissed = animateDismiss ? null : ModalDismissedEvent;
            okModalView.Hide(animateDismiss, modalDismissed);
            okButtonPressed = null;
            okModalTransform.transform.SetParent(rootTransform);
        }

        // Values

        [UIValue("ok-text")]
        internal string OkText
        {
            get => _okText;
            set
            {
                _okText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkText)));
            }
        }

        [UIValue("ok-button-text")]
        internal string OkButtonText
        {
            get => _okButtonText;
            set
            {
                _okButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkButtonText)));
            }
        }

        #endregion

        #region Keyboard

        internal void ShowKeyboard(Transform parent, KeyboardPressed keyboardPressedCallback, bool animateDismiss = true)
        {
            Parse();
            keyboardTransform.transform.SetParent(parent);
            keyboardPressed = keyboardPressedCallback;
            this.animateDismiss = animateDismiss;
            parserParams.EmitEvent("close-keyboard");
            parserParams.EmitEvent("open-keyboard");
        }

        [UIAction("keyboard-enter")]
        private void KeyboardEnter(string keyboardText)
        {
            keyboardPressed?.Invoke(keyboardText);
            Action modalDismissed = animateDismiss ? null : ModalDismissedEvent;
            keyboardModalView.Hide(animateDismiss, modalDismissed);
            keyboardPressed = null;
            keyboardTransform.transform.SetParent(rootTransform);
        }

        #endregion

    }
}
