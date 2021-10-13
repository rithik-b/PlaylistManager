using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistManager.Types
{
    public class DownloadQueueEntry : INotifyPropertyChanged, IProgress<double>
    {
        public readonly BeatSaberPlaylistsLib.Types.IPlaylist playlist;
        public readonly BeatSaberPlaylistsLib.PlaylistManager parentManager;
        public readonly CancellationTokenSource cancellationTokenSource;

        private ImageView bgImage;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIValue("playlist-name")]
        public string PlaylistName => playlist?.Title ?? " ";

        [UIValue("playlist-author")]
        public string PlaylistAuthor => playlist?.Author ?? " ";

        public event PropertyChangedEventHandler PropertyChanged;
        public float Progress { get; private set; }

        public DownloadQueueEntry(BeatSaberPlaylistsLib.Types.IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            this.playlist = playlist;
            this.parentManager = parentManager;
            cancellationTokenSource = new CancellationTokenSource();
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
            playlistCoverView.sprite = playlist.coverImage;
            playlistCoverView.rectTransform.sizeDelta = new Vector2(8, 0);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAuthor)));

            bgImage = playlistCoverView.transform.parent.gameObject.AddComponent<ImageView>();
            bgImage.enabled = true;
            bgImage.sprite = Sprite.Create((new Texture2D(1, 1)), new Rect(0, 0, 1, 1), Vector2.one / 2f);
            bgImage.type = Image.Type.Filled;
            bgImage.fillMethod = Image.FillMethod.Horizontal;
            bgImage.fillAmount = 0;
            bgImage.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;

            Report(Progress);
        }

        [UIAction("abort-clicked")]
        public void AbortDownload() => cancellationTokenSource.Cancel();

        public void Report(double progressDouble)
        {
            if (bgImage != null)
            {
                Progress = (float)progressDouble;
                Color color = SongCore.Utilities.HSBColor.ToColor(new SongCore.Utilities.HSBColor(Mathf.PingPong(Progress * 0.35f, 1), 1, 1));
                color.a = 0.35f;
                bgImage.color = color;
                bgImage.fillAmount = Progress;
            }
        }
    }
}
