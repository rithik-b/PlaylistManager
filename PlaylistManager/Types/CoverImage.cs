using BeatSaberPlaylistsLib.Types;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * Yoinked from Playlists Lib with some changes
 * Original Author: Zingabopp
 */

namespace PlaylistManager.Types
{
    public class CoverImage : IDeferredSpriteLoad
    {
        public string Path { get; private set; }
        private Sprite _sprite;
        private bool SpriteLoadQueued;

        public bool SpriteWasLoaded { get; private set; }
        public bool Blacklist { get; private set; }
        public event EventHandler SpriteLoaded;

        private static readonly object _loaderLock = new object();
        private static bool CoroutineRunning = false;
        private static readonly Queue<Action> SpriteQueue = new Queue<Action>();

        public CoverImage(string path)
        {
            Path = path;
            SpriteWasLoaded = false;
            Blacklist = false;
            SpriteLoadQueued = false;
        }

        public Sprite Sprite
        {
            get
            {
                if (_sprite == null)
                {
                    if (!SpriteLoadQueued)
                    {
                        SpriteLoadQueued = true;
                        QueueLoadSprite(this);
                    }
                    return BeatSaberMarkupLanguage.Utilities.ImageResources.WhitePixel;
                }
                return _sprite;
            }
        }

        public static YieldInstruction LoadWait = new WaitForEndOfFrame();

        private static void QueueLoadSprite(CoverImage coverImage)
        {
            SpriteQueue.Enqueue(() =>
            {
                try
                {
                    using (FileStream imageStream = File.Open(coverImage.Path, FileMode.Open))
                    {
                        byte[] imageBytes = new byte[imageStream.Length];
                        imageStream.Read(imageBytes, 0, (int)imageStream.Length);
                        coverImage._sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                        if (coverImage._sprite != null)
                        {
                            coverImage.SpriteWasLoaded = true;
                        }
                        else
                        {
                            Plugin.Log.Critical("Could not load " + coverImage.Path);
                            coverImage.SpriteWasLoaded = false;
                            coverImage.Blacklist = true;
                        }
                        coverImage.SpriteLoaded?.Invoke(coverImage, null);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Critical("Could not load " + coverImage.Path + "\nException message: " + e.Message);
                    coverImage.SpriteWasLoaded = false;
                    coverImage.Blacklist = true;
                    coverImage.SpriteLoaded?.Invoke(coverImage, null);
                }
            });

            if (!CoroutineRunning)
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }

        private static IEnumerator<YieldInstruction> SpriteLoadCoroutine()
        {
            lock (_loaderLock)
            {
                if (CoroutineRunning)
                    yield break;
                CoroutineRunning = true;
            }
            while (SpriteQueue.Count > 0)
            {
                yield return LoadWait;
                var loader = SpriteQueue.Dequeue();
                loader?.Invoke();
            }
            CoroutineRunning = false;
            if (SpriteQueue.Count > 0)
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }
    }
}
