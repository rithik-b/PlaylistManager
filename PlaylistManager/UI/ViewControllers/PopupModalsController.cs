using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.ComponentModel;
using System.Reflection;
using BeatSaberMarkupLanguage.Components;
using UnityEngine;

namespace PlaylistManager.UI
{
    public class PopupModalsController : NotifiableBase
    {
        private readonly MainMenuViewController mainMenuViewController;
        private bool parsed;
        
        private Action? yesButtonPressed;
        private Action? noButtonPressed;
        private Action? okButtonPressed;

        private Action<string>? keyboardPressed;

        private string yesNoText = "";
        private string checkboxText = "";
        private string yesButtonText = "Yes";
        private string noButtonText = "No";

        private bool checkboxValue;
        private bool checkboxActive;

        private string okText = "";
        private string okButtonText = "Ok";

        private string loadingText = "";

        private string keyboardText = "";

        [UIComponent("root")]
        private readonly RectTransform rootTransform = null!;

        [UIComponent("yes-no-modal")]
        private readonly RectTransform yesNoModalTransform = null!;

        [UIComponent("yes-no-modal")]
        private ModalView yesNoModalView = null!;

        private Vector3 yesNoModalPosition;

        [UIComponent("ok-modal")]
        private readonly RectTransform okModalTransform = null!;

        [UIComponent("ok-modal")]
        private ModalView okModalView = null!;

        private Vector3 okModalPosition;

        [UIComponent("loading-modal")]
        private readonly RectTransform loadingModalTransform = null!;

        private Vector3 loadingModalPosition;

        [UIComponent("keyboard")]
        private readonly RectTransform keyboardTransform = null!;

        [UIComponent("keyboard")]
        private ModalView keyboardModalView = null!;

        [UIParams]
        private readonly BSMLParserParams parserParams = null!;

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

        internal void ShowYesNoModal(Transform parent, string text, Action? yesButtonPressedCallback,
            string yesButtonText = "Yes", string noButtonText = "No", Action? noButtonPressedCallback = null,
            bool animateParentCanvas = true, string checkboxText = "")
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

            Accessors.AnimateCanvasAccessor(ref yesNoModalView) = animateParentCanvas;
            Accessors.ViewValidAccessor(ref yesNoModalView) = false; // Need to do this to show the animation after parent changes

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
            get => yesNoText;
            set
            {
                yesNoText = value;
                NotifyPropertyChanged(nameof(YesNoText));
            }
        }

        [UIValue("yes-button-text")]
        private string YesButtonText
        {
            get => yesButtonText;
            set
            {
                yesButtonText = value;
                NotifyPropertyChanged(nameof(YesButtonText));
            }
        }

        [UIValue("no-button-text")]
        private string NoButtonText
        {
            get => noButtonText;
            set
            {
                noButtonText = value;
                NotifyPropertyChanged(nameof(NoButtonText));
            }
        }

        [UIValue("checkbox-text")]
        private string CheckboxText
        {
            get => checkboxText;
            set
            {
                checkboxText = value;
                NotifyPropertyChanged(nameof(CheckboxText));
            }
        }

        [UIValue("checkbox-active")]
        private bool CheckboxActive
        {
            get => checkboxActive;
            set
            {
                checkboxActive = value;
                NotifyPropertyChanged(nameof(CheckboxActive));
            }
        }

        [UIValue("checkbox")]
        private string Checkbox => CheckboxValue ? "☑" : "⬜";

        public bool CheckboxValue
        {
            get => checkboxValue;
            private set
            {
                checkboxValue = value;
                NotifyPropertyChanged(nameof(Checkbox));
            }
        }

        #endregion

        #region Ok Modal

        // Methods

        private void ShowOkModal(OkPopupContents popupContents)
        {
            ShowOkModal(popupContents.parent, popupContents.message, popupContents.buttonPressedCallback, popupContents.okButtonText, popupContents.animateParentCanvas);
        }

        internal void ShowOkModal(Transform parent, string text, Action? buttonPressedCallback,
            string okButtonText = "Ok", bool animateParentCanvas = true)
        {
            Parse();
            okModalTransform.localPosition = okModalPosition;
            okModalTransform.transform.SetParent(parent);

            OkText = text;
            OkButtonText = okButtonText;
            okButtonPressed = buttonPressedCallback;

            Accessors.AnimateCanvasAccessor(ref okModalView) = animateParentCanvas;
            Accessors.ViewValidAccessor(ref yesNoModalView) = false;

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
            get => okText;
            set
            {
                okText = value;
                NotifyPropertyChanged(nameof(OkText));
            }
        }

        [UIValue("ok-button-text")]
        internal string OkButtonText
        {
            get => okButtonText;
            set
            {
                okButtonText = value;
                NotifyPropertyChanged(nameof(OkButtonText));
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

            Accessors.AnimateCanvasAccessor(ref okModalView) = animateParentCanvas;
            Accessors.ViewValidAccessor(ref yesNoModalView) = false;

            parserParams.EmitEvent("close-loading");
            parserParams.EmitEvent("open-loading");
        }

        internal void DismissLoadingModal() => parserParams.EmitEvent("close-loading");

        [UIValue("loading-text")]
        private string LoadingText
        {
            get => loadingText;
            set
            {
                loadingText = value;
                NotifyPropertyChanged(nameof(LoadingText));
            }
        }

        #endregion

        #region Keyboard

        // Methods

        internal void ShowKeyboard(Transform parent, Action<string>? keyboardPressedCallback,
            string keyboardText = "", bool animateParentCanvas = true)
        {
            Parse(); 
            keyboardTransform.transform.SetParent(rootTransform);
            keyboardTransform.transform.SetParent(parent);

            KeyboardText = keyboardText;

            keyboardPressed = keyboardPressedCallback;

            Accessors.AnimateCanvasAccessor(ref keyboardModalView) = animateParentCanvas;
            Accessors.ViewValidAccessor(ref yesNoModalView) = false;

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
            get => keyboardText;
            set => keyboardText = value;
        }

        #endregion

    }
}
