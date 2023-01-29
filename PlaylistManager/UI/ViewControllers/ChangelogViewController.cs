using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        [Inject]
        private readonly ISiraSyncService siraSyncService = null!;
        private string _changelog = "";

        [UIAction("#post-parse")]
        private async void PostParse()
        {
            var rawChangelog = await siraSyncService.LatestChangelog();
            Changelog = await Task.Run(() => rawChangelog != null ? MarkdownParse(rawChangelog) : "Could not load changelog.");
        }

        private string MarkdownParse(string original)
        {
            // We do a little filtering using regex
            original = Regex.Replace(original, @"!\[.*\]\(.*\)\r\n", ""); // No images with line breaks
            original = Regex.Replace(original, @"!\[.*\]\(.*\)", ""); // No images
            original = Regex.Replace(original, @"(\[)(.*)(\]\(.*\))", "$2"); // No hyperlinks

            // Newlines need to be doubled
            original = original.Replace("\n", "\n\n");

            // I will not need more than 3 headings
            original = Regex.Replace(original, @"(### )(.*)", "<size=5.75>$2</size>\n"); // Heading 3
            original = Regex.Replace(original, @"(## )(.*)", "<size=6>$2</size>\n<color=#ffffff80>________________________________________________________</color>"); // Heading 2
            original = Regex.Replace(original, @"(# )(.*)", "<size=7>$2</size>\n<color=#ffffff80>________________________________________________________</color>"); // Heading 1

            return original;
        }

        [UIValue("is-loading")]
        private bool IsLoading => string.IsNullOrEmpty(Changelog);

        [UIValue("loaded")]
        private bool Loaded => !string.IsNullOrEmpty(Changelog);

        [UIValue("changelog")]
        private string Changelog
        {
            get => _changelog;
            set
            {
                _changelog = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLoading));
                NotifyPropertyChanged(nameof(Loaded));
            }
        }
    }
}
