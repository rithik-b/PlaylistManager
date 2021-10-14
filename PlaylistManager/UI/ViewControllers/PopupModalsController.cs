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
        private readonly LevelSelectionNavigationController levelSelectionNavigationController;
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

        [UIComponent("keyboard")]
        private readonly RectTransform keyboardTransform;

        [UIComponent("keyboard")]
        private ModalView keyboardModalView;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public PopupModalsController(LevelSelectionNavigationController levelSelectionNavigationController)
        {
            this.levelSelectionNavigationController = levelSelectionNavigationController;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PopupModals.bsml"), levelSelectionNavigationController.gameObject, this);
                yesNoModalPosition = yesNoModalTransform.position;
                okModalPosition = okModalTransform.position;
                parsed = true;
            }
        }

        #region Yes/No Modal

        // Methods

        internal void ShowYesNoModal(PopupContents popupContents)
        {
            ShowYesNoModal(popupContents.parent, popupContents.message, popupContents.yesButtonPressedCallback, popupContents.yesButtonText,
                popupContents.noButtonText, popupContents.noButtonPressedCallback, popupContents.animateParentCanvas, popupContents.checkboxText);
        }

        internal void ShowYesNoModal(Transform parent, string text, Action yesButtonPressedCallback, string yesButtonText = "Yes", string noButtonText = "No", Action noButtonPressedCallback = null, bool animateParentCanvas = true, string checkboxText = "")
        {
            Parse();
            yesNoModalTransform.position = yesNoModalPosition;
            keyboardTransform.transform.SetParent(rootTransform);
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

            parserParams.EmitEvent("close-yes-no");
            parserParams.EmitEvent("open-yes-no");
        }

        internal void HideYesNoModal() => parserParams.EmitEvent("close-yes-no");

        [UIAction("yes-button-pressed")]
        private void YesButtonPressed()
        {
            yesButtonPressed?.Invoke();
            yesButtonPressed = null;
            yesNoModalTransform.transform.SetParent(rootTransform);
        }

        [UIAction("no-button-pressed")]
        private void NoButtonPressed()
        {
            noButtonPressed?.Invoke();
            noButtonPressed = null;
            yesNoModalTransform.transform.SetParent(rootTransform);
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

        internal void ShowOkModal(Transform parent, string text, Action buttonPressedCallback, string okButtonText = "Ok", bool animateParentCanvas = true)
        {
            Parse();
            okModalTransform.position = okModalPosition;
            keyboardTransform.transform.SetParent(rootTransform);
            okModalTransform.transform.SetParent(parent);

            OkText = text;
            OkButtonText = okButtonText;
            okButtonPressed = buttonPressedCallback;

            Accessors.AnimateCanvasAccessor(ref okModalView) = animateParentCanvas;

            parserParams.EmitEvent("close-ok");
            parserParams.EmitEvent("open-ok");
        }

        [UIAction("ok-button-pressed")]
        private void OkButtonPressed()
        {
            okButtonPressed?.Invoke();
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

        // Methods

        internal void ShowKeyboard(Transform parent, Action<string> keyboardPressedCallback, string keyboardText = "", bool animateParentCanvas = true)
        {
            Parse(); 
            keyboardTransform.transform.SetParent(rootTransform);
            keyboardTransform.transform.SetParent(parent);

            KeyboardText = keyboardText;

            keyboardPressed = keyboardPressedCallback;

            Accessors.AnimateCanvasAccessor(ref keyboardModalView) = animateParentCanvas;

            parserParams.EmitEvent("close-keyboard");
            parserParams.EmitEvent("open-keyboard");
        }

        [UIAction("keyboard-enter")]
        private void KeyboardEnter(string keyboardText)
        {
            keyboardPressed?.Invoke(keyboardText);
            keyboardPressed = null;
            keyboardTransform.transform.SetParent(rootTransform);
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
