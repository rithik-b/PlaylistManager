using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using SiraUtil.Web.SiraSync;
using Zenject;

namespace PlaylistManager.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\ChangelogView.bsml")]
    [ViewDefinition("PlaylistManager.UI.Views.ChangelogView.bsml")]
    internal class ChangelogViewController : BSMLAutomaticViewController
    {
        private ISiraSyncService siraSyncService;
        private string _changelog;

        [Inject]
        public void Construct(ISiraSyncService siraSyncService)
        {
            this.siraSyncService = siraSyncService;
        }

        [UIAction("#post-parse")]
        private async void PostParse()
        {
            Changelog = await siraSyncService.LatestChangelog();
        }

        [UIValue("is-loading")]
        private bool IsLoading => string.IsNullOrEmpty(Changelog);

        [UIValue("changelog")]
        private string Changelog
        {
            get => _changelog;
            set
            {
                _changelog = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }
    }
}
