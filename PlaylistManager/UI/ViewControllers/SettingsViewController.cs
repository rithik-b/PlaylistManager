using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
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

        internal void Register()
        {
            BSMLSettings.instance.AddSettingsMenu("PlaylistManager", "PlaylistManager.UI.Views.Settings.bsml", instance);
        }

        internal void Unregister()
        {
            BSMLSettings.instance.RemoveSettingsMenu(instance);
        }
    }
}
