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
        private ChangelogViewController changelogViewController;

        [Inject]
        public void Construct(SettingsViewController settingsViewController, ChangelogViewController changelogViewController)
        {
            this.settingsViewController = settingsViewController;
            this.changelogViewController = changelogViewController;
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
            ProvideInitialViewControllers(settingsViewController, changelogViewController);
        }

        public void PresentFlowCoordinator(FlowCoordinator parentFlowCoordinator)
        {
            this.parentFlowCoordinator = parentFlowCoordinator;
            parentFlowCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        private void DismissFlowCoordinator()
        {
            parentFlowCoordinator.DismissFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}
