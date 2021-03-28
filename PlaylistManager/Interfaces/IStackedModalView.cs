using System;
namespace PlaylistManager.Interfaces
{
    // Interface for stacked modals (modals that appear on top of modals)
    interface IStackedModalView
    {
        public event Action ModalDismissedEvent;
    }
}
