using System;
using UnityEngine;

namespace PlaylistManager.Types
{
    public abstract class PopupContents
    {
        public readonly string message;
        public bool animateParentCanvas;

        public PopupContents(string message, bool animateParentCanvas = true)
        {
            this.message = message;
            this.animateParentCanvas = animateParentCanvas;
        }
    }

    public class YesNoPopupContents : PopupContents
    {
        public readonly Action yesButtonPressedCallback;
        public readonly string yesButtonText;
        public readonly Action? noButtonPressedCallback;
        public readonly string noButtonText;
        public readonly string checkboxText;

        public YesNoPopupContents(string message, Action yesButtonPressedCallback, string yesButtonText = "Yes", string noButtonText = "No",
            Action? noButtonPressedCallback = null, bool animateParentCanvas = true, string checkboxText = "") : base(message, animateParentCanvas)
        {
            this.yesButtonPressedCallback = yesButtonPressedCallback;
            this.yesButtonText = yesButtonText;
            this.noButtonPressedCallback = noButtonPressedCallback;
            this.noButtonText = noButtonText;
            this.checkboxText = checkboxText;
        }
    }

    public class OkPopupContents : PopupContents
    {
        public readonly Action? buttonPressedCallback;
        public readonly string okButtonText;

        public OkPopupContents(string message, Action? buttonPressedCallback, string okButtonText = "Ok", bool animateParentCanvas = true)
            : base(message, animateParentCanvas)
        {
            this.buttonPressedCallback = buttonPressedCallback;
            this.okButtonText = okButtonText;
        }
    }
}
