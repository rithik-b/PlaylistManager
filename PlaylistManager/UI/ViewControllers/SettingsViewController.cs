using BeatSaberMarkupLanguage.Attributes;
using PlaylistManager.Configuration;

namespace PlaylistManager.UI
{
    class SettingsViewController : PersistentSingleton<SettingsViewController>
    {
        [UIValue("author-name")]
        public string AuthorName
        {
            get => PluginConfig.Instance.AuthorName;
            set
            {
                PluginConfig.Instance.AuthorName = value;
            }
        }

        [UIValue("no-image")]
        public bool DefaultImageDisabled
        {
            get => PluginConfig.Instance.DefaultImageDisabled;
            set
            {
                PluginConfig.Instance.DefaultImageDisabled = value;
            }
        }
    }
}
