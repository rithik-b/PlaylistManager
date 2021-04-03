using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Settings;
using PlaylistManager.Configuration;
using System;
using System.ComponentModel;
using Zenject;

namespace PlaylistManager.UI
{
    public class SettingsViewController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        [UIComponent("name-setting")]
        private readonly StringSetting nameSetting;

        public event PropertyChangedEventHandler PropertyChanged;

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
            set
            {
                PluginConfig.Instance.AutomaticAuthorName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutomaticAuthorName)));
            }
        }

        [UIValue("no-image")]
        public bool DefaultImageDisabled
        {
            get => PluginConfig.Instance.DefaultImageDisabled;
            set => PluginConfig.Instance.DefaultImageDisabled = value;
        }

        [UIValue("allow-duplicates")]
        public bool DefaultAllowDuplicates
        {
            get => PluginConfig.Instance.DefaultAllowDuplicates;
            set => PluginConfig.Instance.DefaultAllowDuplicates = value;
        }

        [UIValue("scroll-speed")]
        public float PlaylistScrollSpeed
        {
            get => PluginConfig.Instance.PlaylistScrollSpeed;
            set => PluginConfig.Instance.PlaylistScrollSpeed = value;
        }

        public void Initialize() => BSMLSettings.instance.AddSettingsMenu(nameof(PlaylistManager), "PlaylistManager.UI.Views.Settings.bsml", this);
        public void Dispose() => BSMLSettings.instance.RemoveSettingsMenu(this);
    }
}
