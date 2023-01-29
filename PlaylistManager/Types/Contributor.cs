using System;
using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using UnityEngine;

namespace PlaylistManager.Types
{
    internal class Contributor : NotifiableBase
    {
        [UIValue("name")]
        public string name { get; private set; }

        [UIValue("role")]
        public string role { get; private set; }

        private string iconPath { get; }

        [UIComponent("icon")]
        private readonly ImageView iconImage = null!;

        private string? youtube { get; }

        [UIValue("youtube-active")]
        private bool YoutubeActive => !string.IsNullOrEmpty(youtube);

        private string? twitch { get; }

        [UIValue("twitch-active")]
        private bool TwitchActive => !string.IsNullOrEmpty(twitch);

        private string? github { get; }

        [UIValue("github-active")]
        private bool GithubActive => !string.IsNullOrEmpty(github);

        private string? kofi { get; }

        [UIValue("kofi-active")]
        private bool KofiActive => !string.IsNullOrEmpty(kofi);

        public event Action<string>? OpenURL;
        
        public Contributor(string name, string role, string icon, string? youtube = null, string? twitch = null, string? github = null, string? kofi = null)
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
            NotifyPropertyChanged(nameof(name));
            NotifyPropertyChanged(nameof(role));
            iconImage.sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly(iconPath);
            NotifyPropertyChanged(nameof(YoutubeActive));
            NotifyPropertyChanged(nameof(TwitchActive));
            NotifyPropertyChanged(nameof(GithubActive));
            NotifyPropertyChanged(nameof(KofiActive));
        }

        [UIAction("youtube-click")]
        private void YoutubeClicked() => OpenURL?.Invoke(youtube!);

        [UIAction("twitch-click")]
        private void TwitchClicked() => OpenURL?.Invoke(twitch!);

        [UIAction("github-click")]
        private void GithubClicked() => OpenURL?.Invoke(github!);

        [UIAction("kofi-click")]
        private void KofiClicked() => OpenURL?.Invoke(kofi!);
    }
}
