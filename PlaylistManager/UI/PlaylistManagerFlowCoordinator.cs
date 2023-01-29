using HMUI;
using System;
using Zenject;
using BeatSaberMarkupLanguage;

namespace PlaylistManager.UI
{
    internal class PlaylistManagerFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private FlowCoordinator? parentFlowCoordinator;
        
        [Inject]
        private readonly SettingsViewController settingsViewController = null!;
        
        [Inject]
        private readonly ChangelogViewController changelogViewController = null!;
        
        [Inject]
        private readonly ContributorsViewController contributorsViewController = null!;

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
            if (parentFlowCoordinator != null)
                parentFlowCoordinator.DismissFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}
