using System;
using UnityEngine;

namespace PlaylistManager.Types
{
    public class PopupContents
    {
        public Transform parent;
        public readonly string message;
        public readonly Action yesButtonPressedCallback;
        public readonly string yesButtonText;
        public readonly Action noButtonPressedCallback;
        public readonly string noButtonText;
        public readonly bool animateParentCanvas;
        public readonly string checkboxText;

        public PopupContents(string message, Action yesButtonPressedCallback, string yesButtonText = "Yes", string noButtonText = "No",
            Action noButtonPressedCallback = null, bool animateParentCanvas = true, string checkboxText = "")
        {
            this.message = message;
            this.yesButtonPressedCallback = yesButtonPressedCallback;
            this.yesButtonText = yesButtonText;
            this.noButtonPressedCallback = noButtonPressedCallback;
            this.noButtonText = noButtonText;
            this.animateParentCanvas = animateParentCanvas;
            this.checkboxText = checkboxText;
        }
    }
}
