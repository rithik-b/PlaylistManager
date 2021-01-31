using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using PlaylistManager.Configuration;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    class SettingsViewController : IInitializable, IDisposable
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

        public void Initialize()
        {
            BSMLSettings.instance.AddSettingsMenu(nameof(PlaylistManager), "PlaylistManager.UI.Views.Settings.bsml", this);
        }

        public void Dispose()
        {
            BSMLSettings.instance.RemoveSettingsMenu(this);
        }
    }
}
