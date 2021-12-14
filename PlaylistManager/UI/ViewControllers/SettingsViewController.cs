using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using PlaylistManager.Configuration;
using System;
using Zenject;

namespace PlaylistManager.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\SettingsView.bsml")]
    [ViewDefinition("PlaylistManager.UI.Views.SettingsView.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private MenuTransitionsHelper menuTransitionsHelper;

        private bool _defaultImageDisabled;
        private bool _defaultAllowDuplicates;
        private string _authorName;
        private bool _automaticAuthorName;
        private bool _playlistHoverHints;
        private float _playlistScrollSpeed;
        private bool _showDownloadIcon;
        private bool _blurredArt;
        private bool _foldersDisabled;
        private int _syncOption;
        private bool _downloadDuringGameplay;
        private bool _driveFullProtection;
        private bool _easterEggs;

        public event Action DismissFlowEvent;
        public event Action NameFetchRequestedEvent;

        [Inject]
        public void Construct(MenuTransitionsHelper menuTransitionsHelper)
        {
            this.menuTransitionsHelper = menuTransitionsHelper;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            // Get all values from config
            DefaultImageDisabled = PluginConfig.Instance.DefaultImageDisabled;
            DefaultAllowDuplicates = PluginConfig.Instance.DefaultAllowDuplicates;
            AutomaticAuthorName = PluginConfig.Instance.AutomaticAuthorName;
            AuthorName = PluginConfig.Instance.AuthorName;
            PlaylistHoverHints = PluginConfig.Instance.PlaylistHoverHints;
            PlaylistScrollSpeed = PluginConfig.Instance.PlaylistScrollSpeed;
            ShowDownloadIcon = PluginConfig.Instance.ShowDownloadIcon;
            BlurredArt = PluginConfig.Instance.BlurredArt;
            FoldersDisabled = PluginConfig.Instance.FoldersDisabled;
            SyncOption = (int)PluginConfig.Instance.SyncOption;
            DownloadDuringGameplay = PluginConfig.Instance.DownloadDuringGameplay;
            DriveFullProtection = PluginConfig.Instance.DriveFullProtection;
            EasterEggs = PluginConfig.Instance.EasterEggs;
        }

        [UIAction("cancel-click")]
        private void CancelClicked() => DismissFlowEvent?.Invoke();

        [UIAction("ok-click")]
        private void OkClicked()
        {
            bool fetchName = !PluginConfig.Instance.AutomaticAuthorName && AutomaticAuthorName;
            bool softRestart = SoftRestart;

            // Save all values to config
            PluginConfig.Instance.DefaultImageDisabled = DefaultImageDisabled;
            PluginConfig.Instance.DefaultAllowDuplicates = DefaultAllowDuplicates;
            PluginConfig.Instance.AutomaticAuthorName = AutomaticAuthorName;
            PluginConfig.Instance.AuthorName = AuthorName;
            PluginConfig.Instance.PlaylistHoverHints = PlaylistHoverHints;
            PluginConfig.Instance.PlaylistScrollSpeed = PlaylistScrollSpeed;
            PluginConfig.Instance.ShowDownloadIcon = ShowDownloadIcon;
            PluginConfig.Instance.BlurredArt = BlurredArt;
            PluginConfig.Instance.FoldersDisabled = FoldersDisabled;
            PluginConfig.Instance.SyncOption = (PluginConfig.SyncOptions)SyncOption;
            PluginConfig.Instance.DownloadDuringGameplay = DownloadDuringGameplay;
            PluginConfig.Instance.DriveFullProtection = DriveFullProtection;
            PluginConfig.Instance.EasterEggs = EasterEggs;

            if (softRestart)
            {
                menuTransitionsHelper.RestartGame();
            }
            else
            {
                DismissFlowEvent?.Invoke();
                if (fetchName)
                {
                    NameFetchRequestedEvent?.Invoke();
                }
            }
        }

        [UIAction("sync-formatter")]
        private string PositionFormatter(int index)
        {
            return ((PluginConfig.SyncOptions)index).ToString();
        }

        #region Default Playlist Settings

        [UIValue("no-image")]
        private bool DefaultImageDisabled
        {
            get => _defaultImageDisabled;
            set
            {
                _defaultImageDisabled = value;
                NotifyPropertyChanged(nameof(DefaultImageDisabled));
            }
        }

        [UIValue("allow-duplicates")]
        private bool DefaultAllowDuplicates
        {
            get => _defaultAllowDuplicates;
            set
            {
                _defaultAllowDuplicates = value;
                NotifyPropertyChanged(nameof(DefaultAllowDuplicates));
            }
        }

        [UIValue("auto-name")]
        private bool AutomaticAuthorName
        {
            get => _automaticAuthorName;
            set
            {
                _automaticAuthorName = value;
                NotifyPropertyChanged(nameof(AutomaticAuthorName));
                NotifyPropertyChanged(nameof(NameActive));
            }
        }

        [UIValue("author-name")]
        private string AuthorName
        {
            get => _authorName;
            set
            {
                _authorName = value;
                NotifyPropertyChanged(nameof(AuthorName));
            }
        }

        [UIValue("name-active")]
        private bool NameActive => !AutomaticAuthorName;

        #endregion

        #region User Interface Settings

        [UIValue("hover-hint")]
        private bool PlaylistHoverHints
        {
            get => _playlistHoverHints;
            set
            {
                _playlistHoverHints = value;
                NotifyPropertyChanged(nameof(PlaylistHoverHints));
                NotifyPropertyChanged(nameof(SoftRestart));
            }
        }

        [UIValue("scroll-speed")]
        private float PlaylistScrollSpeed
        {
            get => _playlistScrollSpeed;
            set
            {
                _playlistScrollSpeed = value;
                NotifyPropertyChanged(nameof(PlaylistScrollSpeed));
                NotifyPropertyChanged(nameof(SoftRestart));
            }
        }

        [UIValue("download-icon")]
        private bool ShowDownloadIcon
        {
            get => _showDownloadIcon;
            set
            {
                _showDownloadIcon = value;
                NotifyPropertyChanged(nameof(ShowDownloadIcon));
            }
        }

        [UIValue("blurred-art")]
        private bool BlurredArt
        {
            get => _blurredArt;
            set
            {
                _blurredArt = value;
                NotifyPropertyChanged(nameof(BlurredArt));
            }
        }

        [UIValue("no-folders")]
        private bool FoldersDisabled
        {
            get => _foldersDisabled;
            set
            {
                _foldersDisabled = value;
                NotifyPropertyChanged(nameof(FoldersDisabled));
                NotifyPropertyChanged(nameof(SoftRestart));
            }
        }

        #endregion

        #region Other Settings

        [UIValue("sync-option")]
        private int SyncOption
        {
            get => _syncOption;
            set
            {
                _syncOption = value;
                NotifyPropertyChanged(nameof(SyncOption));
            }
        }

        [UIValue("gameplay-download")]
        private bool DownloadDuringGameplay
        {
            get => _downloadDuringGameplay;
            set
            {
                _downloadDuringGameplay = value;
                NotifyPropertyChanged(nameof(DownloadDuringGameplay));
            }
        }

        [UIValue("drive-protection")]
        private bool DriveFullProtection
        {
            get => _driveFullProtection;
            set
            {
                _driveFullProtection = value;
                NotifyPropertyChanged(nameof(DriveFullProtection));
            }
        }

        [UIValue("easter-eggs")]
        private bool EasterEggs
        {
            get => _easterEggs;
            set
            {
                _easterEggs = value;
                NotifyPropertyChanged(nameof(EasterEggs));
            }
        }

        #endregion

        [UIValue("soft-restart")]
        private bool SoftRestart => PlaylistHoverHints != PluginConfig.Instance.PlaylistHoverHints || PlaylistScrollSpeed != PluginConfig.Instance.PlaylistScrollSpeed ||
            FoldersDisabled != PluginConfig.Instance.FoldersDisabled;
    }
}
