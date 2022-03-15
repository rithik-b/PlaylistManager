using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using System;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistManager.Types
{
    public class DownloadQueueEntry : INotifyPropertyChanged, IProgress<double>, IProgress<float>
    {
        public readonly IPlaylist playlist;
        public readonly BeatSaberPlaylistsLib.PlaylistManager parentManager;
        public CancellationTokenSource cancellationTokenSource;
        public bool Aborted;
        public event Action<DownloadQueueEntry> DownloadAbortedEvent;

        private ImageView bgImage;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIValue("playlist-name")]
        public string PlaylistName => playlist?.packName ?? "";

        [UIValue("playlist-subtext")]
        public string PlaylistSubtext => (playlist?.Author ?? "") + $" [{completedLevels}/{missingLevels} downloaded]";

        public event PropertyChangedEventHandler PropertyChanged;

        private double progress;
        public double Progress
        {
            get => progress;
            private set
            {
                progress = value;
                if (bgImage != null)
                {
                    var color = SongCore.Utilities.HSBColor.ToColor(new SongCore.Utilities.HSBColor(Mathf.PingPong((float) (Progress * 0.35f), 1), 1, 1));
                    color.a = 0.35f;
                    bgImage.color = color;
                    bgImage.fillAmount = (float) Progress;
                }
            }
        }
        private int completedLevels;
        private int missingLevels = 1; // Just in case a divide by 0 happens
        
        public DownloadQueueEntry(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            this.playlist = playlist;
            this.parentManager = parentManager;
            cancellationTokenSource = new CancellationTokenSource();
            Aborted = false;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            if (playlistCoverView == null)
            {
                return;
            }

            var filter = playlistCoverView.gameObject.AddComponent<AspectRatioFitter>();
            filter.aspectRatio = 1f;
            filter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;

            if (playlist is IDeferredSpriteLoad deferredSpriteLoad)
            {
                if (!deferredSpriteLoad.SpriteWasLoaded)
                {
                    deferredSpriteLoad.SpriteLoaded += OnSpriteLoad;
                }
            }

            playlistCoverView.sprite = playlist.coverImage;
            playlistCoverView.rectTransform.sizeDelta = new Vector2(8, 0);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistSubtext)));

            bgImage = playlistCoverView.transform.parent.gameObject.AddComponent<ImageView>();
            bgImage.enabled = true;
            bgImage.sprite = Sprite.Create((new Texture2D(1, 1)), new Rect(0, 0, 1, 1), Vector2.one / 2f);
            bgImage.type = Image.Type.Filled;
            bgImage.fillMethod = Image.FillMethod.Horizontal;
            bgImage.fillAmount = 0;
            bgImage.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;

            Progress = progress;
        }

        private void OnSpriteLoad(object sender, EventArgs e)
        {
            if (sender is IDeferredSpriteLoad deferredSpriteLoad)
            {
                deferredSpriteLoad.SpriteLoaded -= OnSpriteLoad;
                playlistCoverView.sprite = deferredSpriteLoad.Sprite;
            }
        }

        [UIAction("abort-clicked")]
        public void AbortDownload()
        {
            cancellationTokenSource.Cancel();
            DownloadAbortedEvent?.Invoke(this);
            Aborted = true;
        }

        public void Report(double value) => Progress = ((double)completedLevels / missingLevels) + (value / missingLevels);
        public void Report(float value) => Report((double)value);
        
        public void SetMissingLevels(int value)
        {
            missingLevels = value;
            completedLevels = 0;
            Progress = 0;
        }

        public void SetTotalProgress(int value)
        {
            completedLevels = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistSubtext)));
        }
    }
}
