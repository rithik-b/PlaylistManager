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
        private ContributorsViewController contributorsViewController;

        [Inject]
        public void Construct(SettingsViewController settingsViewController, ContributorsViewController contributorsViewController)
        {
            this.settingsViewController = settingsViewController;
            this.contributorsViewController = contributorsViewController;
        }

        public void Initialize()
        {
            settingsViewController.DismissFlowEvent += DismissFlowCoordinator;
        }

        public void Dispose()
        {
            settingsViewController.DismissFlowEvent -= DismissFlowCoordinator;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ProvideInitialViewControllers(settingsViewController, null, contributorsViewController);
        }

        public void PresentFlowCoordinator(FlowCoordinator parentFlowCoordinator)
        {
            this.parentFlowCoordinator = parentFlowCoordinator;
            parentFlowCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        private void DismissFlowCoordinator()
        {
            contributorsViewController.gameObject.SetActive(false);
            parentFlowCoordinator.DismissFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}
