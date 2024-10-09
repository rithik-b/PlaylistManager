using System;
using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;

namespace PlaylistManager.Types
{
    internal class Contributor : INotifyPropertyChanged
    {
        [UIValue("name")]
        public string name { get; private set; }

        [UIValue("role")]
        public string role { get; private set; }

        public string iconPath { get; private set; }

        [UIComponent("icon")]
        private readonly ImageView iconImage;

        public string youtube { get; private set; }

        [UIValue("youtube-active")]
        private bool YoutubeActive => !string.IsNullOrEmpty(youtube);

        public string twitch { get; private set; }

        [UIValue("twitch-active")]
        private bool TwitchActive => !string.IsNullOrEmpty(twitch);

        public string github { get; private set; }

        [UIValue("github-active")]
        private bool GithubActive => !string.IsNullOrEmpty(github);

        public string kofi { get; private set; }

        [UIValue("kofi-active")]
        private bool KofiActive => !string.IsNullOrEmpty(kofi);

        public event Action<string> OpenURL;
        public event PropertyChangedEventHandler PropertyChanged;

        public Contributor(string name, string role, string icon, string youtube = null, string twitch = null, string github = null, string kofi = null)
        {
            this.name = name;
            this.role = role;
            iconPath = icon;
            this.youtube = youtube;
            this.twitch = twitch;
            this.github = github;
            this.kofi = kofi;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(name)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(role)));
            iconImage.sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly(iconPath);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(YoutubeActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TwitchActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GithubActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KofiActive)));
        }

        [UIAction("youtube-click")]
        private void YoutubeClicked() => OpenURL?.Invoke(youtube);

        [UIAction("twitch-click")]
        private void TwitchClicked() => OpenURL?.Invoke(twitch);

        [UIAction("github-click")]
        private void GithubClicked() => OpenURL?.Invoke(github);

        [UIAction("kofi-click")]
        private void KofiClicked() => OpenURL?.Invoke(kofi);
    }
}
