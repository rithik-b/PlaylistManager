using System.Text.RegularExpressions;
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

                // We do a little filtering using regex
                _changelog = Regex.Replace(_changelog, @"!\[.*\]\(.*\)", ""); // No images
                _changelog = Regex.Replace(_changelog, @"(\[)(.*)(\]\(.*\))", "$2"); // No hyperlinks

                // I will not need more than 3 headings
                _changelog = Regex.Replace(_changelog, @"(### )(.*)", "<size=5.75>$2</size>\n"); // Heading 3
                _changelog = Regex.Replace(_changelog, @"(## )(.*)", "<size=6>$2</size>\n<color=#ffffff80>________________________________________________________</color>"); // Heading 2
                _changelog = Regex.Replace(_changelog, @"(# )(.*)", "<size=7>$2</size>\n<color=#ffffff80>________________________________________________________</color>"); // Heading 1

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }
    }
}
