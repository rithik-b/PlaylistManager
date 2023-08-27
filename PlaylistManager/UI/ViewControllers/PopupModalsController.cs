using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace PlaylistManager.UI
{
    public class PopupModalsController : INotifyPropertyChanged
    {
        private readonly MainMenuViewController mainMenuViewController;
        private bool parsed;
        public event PropertyChangedEventHandler PropertyChanged;

        private Action yesButtonPressed;
        private Action noButtonPressed;
        private Action okButtonPressed;

        private Action<string> keyboardPressed;

        private string _yesNoText = "";
        private string _checkboxText = "";
        private string _yesButtonText = "Yes";
        private string _noButtonText = "No";

        private bool _checkboxValue = false;
        private bool _checkboxActive = false;

        private string _okText = "";
        private string _okButtonText = "Ok";

        private string _loadingText = "";

        private string _keyboardText = "";

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("yes-no-modal")]
        private readonly RectTransform yesNoModalTransform;

        [UIComponent("yes-no-modal")]
        private ModalView yesNoModalView;

        private Vector3 yesNoModalPosition;

        [UIComponent("ok-modal")]
        private readonly RectTransform okModalTransform;

        [UIComponent("ok-modal")]
        private ModalView okModalView;

        private Vector3 okModalPosition;

        [UIComponent("loading-modal")]
        private readonly RectTransform loadingModalTransform;

        [UIComponent("loading-modal")]
        private ModalView loadingModalView;

        private Vector3 loadingModalPosition;

        [UIComponent("keyboard")]
        private readonly RectTransform keyboardTransform;

        [UIComponent("keyboard")]
        private ModalView keyboardModalView;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public PopupModalsController(MainMenuViewController mainMenuViewController)
        {
            this.mainMenuViewController = mainMenuViewController;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PopupModals.bsml"), mainMenuViewController.gameObject, this);
                yesNoModalPosition = yesNoModalTransform.localPosition;
                okModalPosition = okModalTransform.localPosition;
                loadingModalPosition = loadingModalTransform.localPosition;
                parsed = true;
            }
        }

        internal void ShowModal(PopupContents popupContents)
        {
            if (popupContents is OkPopupContents okPopupContents)
            {
                ShowOkModal(okPopupContents);
            }
            else if (popupContents is YesNoPopupContents yesNoPopupContents)
            {
                ShowYesNoModal(yesNoPopupContents);
            }
        }

        #region Yes/No Modal

        // Methods

        private void ShowYesNoModal(YesNoPopupContents popupContents)
        {
            ShowYesNoModal(popupContents.parent, popupContents.message, popupContents.yesButtonPressedCallback, popupContents.yesButtonText,
                popupContents.noButtonText, popupContents.noButtonPressedCallback, popupContents.animateParentCanvas, popupContents.checkboxText);
        }

        internal void ShowYesNoModal(Transform parent, string text, Action yesButtonPressedCallback, string yesButtonText = "Yes", string noButtonText = "No", Action noButtonPressedCallback = null, bool animateParentCanvas = true, string checkboxText = "")
        {
            Parse();
            yesNoModalTransform.localPosition = yesNoModalPosition;
            yesNoModalTransform.transform.SetParent(parent);

            YesNoText = text;
            YesButtonText = yesButtonText;
            NoButtonText = noButtonText;

            yesButtonPressed = yesButtonPressedCallback;
            noButtonPressed = noButtonPressedCallback;

            CheckboxText = checkboxText;
            CheckboxValue = false;
            CheckboxActive = !string.IsNullOrEmpty(checkboxText);

            yesNoModalView._animateParentCanvas = animateParentCanvas;
            yesNoModalView._viewIsValid = false; // Need to do this to show the animation after parent changes

            parserParams.EmitEvent("close-yes-no");
            parserParams.EmitEvent("open-yes-no");
        }

        internal void HideYesNoModal() => parserParams.EmitEvent("close-yes-no");

        [UIAction("yes-button-pressed")]
        private void YesButtonPressed()
        {
            yesButtonPressed?.Invoke();
            yesButtonPressed = null;
        }

        [UIAction("no-button-pressed")]
        private void NoButtonPressed()
        {
            noButtonPressed?.Invoke();
            noButtonPressed = null;
        }

        [UIAction("toggle-checkbox")]
        private void ToggleCheckbox() => CheckboxValue = !CheckboxValue;

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

        [UIValue("checkbox-text")]
        private string CheckboxText
        {
            get => _checkboxText;
            set
            {
                _checkboxText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckboxText)));
            }
        }

        [UIValue("checkbox-active")]
        private bool CheckboxActive
        {
            get => _checkboxActive;
            set
            {
                _checkboxActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckboxActive)));
            }
        }

        [UIValue("checkbox")]
        private string Checkbox => CheckboxValue ? "☑" : "⬜";

        public bool CheckboxValue
        {
            get => _checkboxValue;
            private set
            {
                _checkboxValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Checkbox)));
            }
        }

        #endregion

        #region Ok Modal

        // Methods

        private void ShowOkModal(OkPopupContents popupContents)
        {
            ShowOkModal(popupContents.parent, popupContents.message, popupContents.buttonPressedCallback, popupContents.okButtonText, popupContents.animateParentCanvas);
        }

        internal void ShowOkModal(Transform parent, string text, Action buttonPressedCallback, string okButtonText = "Ok", bool animateParentCanvas = true)
        {
            Parse();
            okModalTransform.localPosition = okModalPosition;
            okModalTransform.transform.SetParent(parent);

            OkText = text;
            OkButtonText = okButtonText;
            okButtonPressed = buttonPressedCallback;

            okModalView._animateParentCanvas = animateParentCanvas;
            yesNoModalView._viewIsValid = false;

            parserParams.EmitEvent("close-ok");
            parserParams.EmitEvent("open-ok");
        }

        [UIAction("ok-button-pressed")]
        private void OkButtonPressed()
        {
            okButtonPressed?.Invoke();
            okButtonPressed = null;
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

        #region Loading Modal

        internal void ShowLoadingModal(Transform parent, string text, bool animateParentCanvas = true)
        {
            Parse();
            loadingModalTransform.localPosition = loadingModalPosition;
            loadingModalTransform.SetParent(parent);

            LoadingText = text;

            okModalView._animateParentCanvas = animateParentCanvas;
            yesNoModalView._viewIsValid = false;

            parserParams.EmitEvent("close-loading");
            parserParams.EmitEvent("open-loading");
        }

        internal void DismissLoadingModal() => parserParams.EmitEvent("close-loading");

        [UIValue("loading-text")]
        private string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingText)));
            }
        }

        #endregion

        #region Keyboard

        // Methods

        internal void ShowKeyboard(Transform parent, Action<string> keyboardPressedCallback, string keyboardText = "", bool animateParentCanvas = true)
        {
            Parse();
            keyboardTransform.transform.SetParent(parent);

            KeyboardText = keyboardText;

            keyboardPressed = keyboardPressedCallback;

            keyboardModalView._animateParentCanvas = animateParentCanvas;
            yesNoModalView._viewIsValid = false;

            parserParams.EmitEvent("close-keyboard");
            parserParams.EmitEvent("open-keyboard");
        }

        [UIAction("keyboard-enter")]
        private void KeyboardEnter(string keyboardText)
        {
            keyboardPressed?.Invoke(keyboardText);
            keyboardPressed = null;
        }

        // Values

        [UIValue("keyboard-text")]
        private string KeyboardText
        {
            get => _keyboardText;
            set => _keyboardText = value;
        }

        #endregion

    }
}
