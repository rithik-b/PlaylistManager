using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Settings;
using PlaylistManager.Configuration;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    class SettingsViewController : IInitializable, IDisposable
    {
        [UIComponent("name-setting")]
        private readonly StringSetting nameSetting;

        [UIValue("author-name")]
        public string AuthorName
        {
            get => PluginConfig.Instance.AuthorName;
            set => PluginConfig.Instance.AuthorName = value;
        }

        [UIValue("auto-name")]
        public bool AutomaticAuthorName
        {
            get
            {
                nameSetting.interactable = !PluginConfig.Instance.AutomaticAuthorName;
                return PluginConfig.Instance.AutomaticAuthorName;
            }
            set => PluginConfig.Instance.AutomaticAuthorName = value;
        }

        [UIValue("no-image")]
        public bool DefaultImageDisabled
        {
            get => PluginConfig.Instance.DefaultImageDisabled;
            set => PluginConfig.Instance.DefaultImageDisabled = value;
        }

        public void Initialize() => BSMLSettings.instance.AddSettingsMenu(nameof(PlaylistManager), "PlaylistManager.UI.Views.Settings.bsml", this);
        public void Dispose() => BSMLSettings.instance.RemoveSettingsMenu(this);

        [UIAction("auto-name-toggled")]
        public void AutoNameToggled(bool value) => nameSetting.interactable = !value;
    }
}
