using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using PlaylistManager.Configuration;
using System;
using System.ComponentModel;
using Zenject;

namespace PlaylistManager.UI
{
    public class SettingsViewController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [UIValue("author-name")]
        public string AuthorName
        {
            get => PluginConfig.Instance.AuthorName;
            set => PluginConfig.Instance.AuthorName = value;
        }

        [UIValue("name-interactable")]
        private bool NameInteractable => !AutomaticAuthorName;

        [UIValue("auto-name")]
        public bool AutomaticAuthorName
        {
            get => PluginConfig.Instance.AutomaticAuthorName;
            set
            {
                PluginConfig.Instance.AutomaticAuthorName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutomaticAuthorName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameInteractable)));
            }
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

        [UIValue("sync-option")]
        private int SyncOption
        {
            get => (int)PluginConfig.Instance.SyncOption;
            set => PluginConfig.Instance.SyncOption = (PluginConfig.SyncOptions)value;
        }

        [UIValue("easter-eggs")]
        public bool EasterEggs
        {
            get => PluginConfig.Instance.EasterEggs;
            set => PluginConfig.Instance.EasterEggs = value;
        }

        [UIValue("no-image")]
        public bool DefaultImageDisabled
        {
            get => PluginConfig.Instance.DefaultImageDisabled;
            set => PluginConfig.Instance.DefaultImageDisabled = value;
        }

        [UIValue("no-folders")]
        public bool FoldersDisabled
        {
            get => PluginConfig.Instance.FoldersDisabled;
            set => PluginConfig.Instance.FoldersDisabled = value;
        }

        [UIValue("no-management")]
        public bool ManagementDisabled
        {
            get => PluginConfig.Instance.ManagementDisabled;
            set => PluginConfig.Instance.ManagementDisabled = value;
        }

        [UIAction("sync-formatter")]
        private string PositionFormatter(int index)
        {
            return ((PluginConfig.SyncOptions)index).ToString();
        }

        public void Initialize() => BSMLSettings.instance.AddSettingsMenu(nameof(PlaylistManager), "PlaylistManager.UI.Views.Settings.bsml", this);
        public void Dispose() => BSMLSettings.instance.RemoveSettingsMenu(this);
    }
}
