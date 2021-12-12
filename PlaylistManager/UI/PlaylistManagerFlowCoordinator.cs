using HMUI;
using System;
using Zenject;
using BeatSaberMarkupLanguage;

namespace PlaylistManager.UI
{
    internal class PlaylistManagerFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private FlowCoordinator parentFlowCoordinator;
        private SettingsViewController settingsViewController;

        [Inject]
        public void Construct(SettingsViewController settingsViewController)
        {
            this.settingsViewController = settingsViewController;
        }

        public void Initialize()
        {
            settingsViewController.DismissFlowEvent += DismissFlowCoordinator;
        }

        public void Dispose()
        {
            settingsViewController.DismissFlowEvent -= DismissFlowCoordinator;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ProvideInitialViewControllers(settingsViewController);
        }

        public void PresentFlowCoordinator(FlowCoordinator parentFlowCoordinator)
        {
            this.parentFlowCoordinator = parentFlowCoordinator;
            parentFlowCoordinator.PresentFlowCoordinator(this);
        }

        private void DismissFlowCoordinator()
        {
            parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
