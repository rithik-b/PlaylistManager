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
        private ContributorsViewController contributorsViewController;

        [Inject]
        public void Construct(SettingsViewController settingsViewController, ChangelogViewController changelogViewController, ContributorsViewController contributorsViewController)
        {
            this.settingsViewController = settingsViewController;
            this.changelogViewController = changelogViewController;
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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ProvideInitialViewControllers(settingsViewController, changelogViewController, contributorsViewController);
        }

        public void PresentFlowCoordinator(FlowCoordinator parentFlowCoordinator)
        {
            this.parentFlowCoordinator = parentFlowCoordinator;
            parentFlowCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        private void DismissFlowCoordinator()
        {
            changelogViewController.gameObject.SetActive(false);
            contributorsViewController.gameObject.SetActive(false);
            parentFlowCoordinator.DismissFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}
