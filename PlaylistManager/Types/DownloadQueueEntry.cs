using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using System;
using System.ComponentModel;
using System.Threading;
using BeatSaberMarkupLanguage.Components;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistManager.Types
{
    /// <summary>
    /// A wrapper class containing download information of a playlist
    /// </summary>
    public class DownloadQueueEntry : NotifiableBase, IProgress<double>, IProgress<float>
    {
        /// <summary>
        /// Playlist to download
        /// </summary>
        public readonly IPlaylist playlist;
        
        /// <summary>
        /// Folder this <see cref="IPlaylist"/> is stored in
        /// </summary>
        public readonly BeatSaberPlaylistsLib.PlaylistManager parentManager;
        
        internal CancellationTokenSource cancellationTokenSource;
        public bool Aborted { get; private set; }
        public event Action<DownloadQueueEntry>? DownloadAbortedEvent;

        private ImageView? bgImage;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView = null!;

        [UIValue("playlist-name")]
        public string PlaylistName => playlist?.packName ?? "";

        [UIValue("playlist-subtext")]
        public string PlaylistSubtext => (playlist?.Author ?? "") + (missingLevels != null ? $" [{completedLevels}/{missingLevels} downloaded]" : " [Download Queued]");
        
        private double progress;
        /// <summary>
        /// Download Progress
        /// </summary>
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
        private int? missingLevels;
        
        /// <summary>
        /// Create a playlist download entry
        /// </summary>
        /// <param name="playlist"><see cref="playlist"/></param>
        /// <param name="parentManager"><see cref="parentManager"/></param>
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
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistSubtext));

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

        public void Report(double value) => Progress = missingLevels != null ? ((double)completedLevels / missingLevels ?? 1) + (value / missingLevels ?? 1) : 0;
        public void Report(float value) => Report((double)value);
        
        internal void SetMissingLevels(int value)
        {
            missingLevels = value;
            completedLevels = 0;
            Progress = 0;
        }

        internal void SetTotalProgress(int value)
        {
            completedLevels = value;
            NotifyPropertyChanged(nameof(PlaylistSubtext));
        }
    }
}
