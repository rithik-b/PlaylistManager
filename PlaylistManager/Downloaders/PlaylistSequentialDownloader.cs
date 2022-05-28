using BeatSaberPlaylistsLib.Types;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using IPA.Loader;
using PlaylistManager.Configuration;
using PlaylistManager.Types;
using SiraUtil.Web;
using SiraUtil.Zenject;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlaylistManager.Utilities;
using SongCore;
using Zenject;

namespace PlaylistManager.Downloaders
{
    internal class PlaylistSequentialDownloader : IInitializable, IDisposable
    {
        private readonly IHttpService siraHttpService;
        private readonly BeatSaver beatSaverInstance;
        private readonly SemaphoreSlim downloadSemaphore;
        private static readonly HashSet<string> ownedHashes = new();
        private DownloadQueueEntry currentDownload;

        private readonly SemaphoreSlim pauseSemaphore;
        private readonly SemaphoreSlim popupSemaphore;
        private bool preferCustomArchiveURL;
        private bool ignoredDiskWarning;
        private bool disposed;

        internal event Action PopupEvent;
        internal event Action QueueUpdatedEvent;
        
        internal static readonly List<object> downloadQueue = new();
        private static readonly LinkedList<BeatSaberPlaylistsLib.Types.Playlist> coversToRefresh = new();

        private PopupContents _pendingPopup;
        internal PopupContents PendingPopup
        {
            get => _pendingPopup;
            private set
            {
                _pendingPopup = value;
                PopupEvent?.Invoke();
            }
        }

        public PlaylistSequentialDownloader(UBinder<Plugin, PluginMetadata> metadata, IHttpService siraHttpService)
        {
            this.siraHttpService = siraHttpService;
            var options = new BeatSaverOptions(metadata.Value.Name, metadata.Value.HVersion.ToString());
            beatSaverInstance = new BeatSaver(options);
            downloadSemaphore = new SemaphoreSlim(1, 1);
            pauseSemaphore = new SemaphoreSlim(0, 1);
            popupSemaphore = new SemaphoreSlim(0, 1);
            PendingPopup = null;
        }

        public void Initialize()
        {
            foreach (var downloadQueueEntry in downloadQueue.OfType<DownloadQueueEntry>())
            {
                downloadQueueEntry.DownloadAbortedEvent += OnDownloadAborted;
            }
            
            foreach (DownloadQueueEntry _ in downloadQueue)
            {
                IterateQueue();
            }
            
            PlaylistDownloader.PlaylistQueuedEvent += OnPlaylistQueued;
        }

        public void Dispose()
        {
            disposed = true;
            if (currentDownload != null && !currentDownload.cancellationTokenSource.IsCancellationRequested)
            {
                currentDownload.cancellationTokenSource.Cancel();
            }

            foreach (var downloadQueueEntry in downloadQueue.OfType<DownloadQueueEntry>())
            {
                downloadQueueEntry.DownloadAbortedEvent -= OnDownloadAborted;
            }
            
            PlaylistDownloader.PlaylistQueuedEvent -= OnPlaylistQueued;
        }

        public void QueuePlaylist(DownloadQueueEntry downloadQueueEntry)
        {
            downloadQueue.Add(downloadQueueEntry);
            OnPlaylistQueued(downloadQueueEntry);
        }
        
        private void OnPlaylistQueued(DownloadQueueEntry downloadQueueEntry)
        {
            downloadQueueEntry.DownloadAbortedEvent += OnDownloadAborted;
            QueueUpdatedEvent?.Invoke();
            IterateQueue();
        }

        private void OnDownloadAborted(DownloadQueueEntry downloadQueueEntry)
        {
            downloadQueueEntry.DownloadAbortedEvent -= OnDownloadAborted;
            downloadQueue.Remove(downloadQueueEntry); 
            QueueUpdatedEvent?.Invoke();
        }

        private async void IterateQueue()
        {
            await downloadSemaphore.WaitAsync();
            if (downloadQueue.Count > 0 && !disposed)
            {
                var toDownload = downloadQueue.OfType<DownloadQueueEntry>().FirstOrDefault();
                await DownloadPlaylist(toDownload);
                if (!disposed)
                {
                    downloadQueue.Remove(toDownload);
                }
                QueueUpdatedEvent?.Invoke();
            }
            downloadSemaphore.Release();
        }

        internal void OnQueueClear()
        {
            if (downloadQueue.Count == 0)
            {
                Loader.SongsLoadedEvent += OnSongsLoaded;
                Loader.Instance.RefreshSongs(false);
                ownedHashes.Clear();
            }
        }

        private void OnSongsLoaded(Loader arg1, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> arg2)
        {
            Loader.SongsLoadedEvent -= OnSongsLoaded;
            foreach (var playlist in coversToRefresh)
            {
                playlist.RaiseCoverImageChangedForDefaultCover();
            }
            coversToRefresh.Clear();
        }

        internal void PauseDownload()
        {
            if (currentDownload != null && !currentDownload.cancellationTokenSource.IsCancellationRequested)
            {
                currentDownload.cancellationTokenSource.Cancel();
            }
        }

        internal void ResumeDownload()
        {
            if (currentDownload != null && pauseSemaphore.CurrentCount == 0)
            {
                pauseSemaphore.Release();
            }
        }

        private async Task DownloadPlaylist(DownloadQueueEntry downloadQueueEntry)
        {
            currentDownload = downloadQueueEntry;
            var missingSongs = PlaylistLibUtils.GetMissingSongs(downloadQueueEntry.playlist, ownedHashes);
            downloadQueueEntry.SetMissingLevels(missingSongs.Count);
            downloadQueueEntry.SetTotalProgress(0);

            preferCustomArchiveURL = true;
            var shownCustomArchiveWarning = false;

            for (var i = 0; i < missingSongs.Count; i++)
            {
                if (preferCustomArchiveURL && missingSongs[i].TryGetCustomData("customArchiveURL", out var outCustomArchiveURL))
                {
                    var customArchiveURL = (string)outCustomArchiveURL;
                    var identifier = PlaylistLibUtils.GetIdentifierForPlaylistSong(missingSongs[i]);
                    if (identifier == "")
                    {
                        continue;
                    }

                    if (!shownCustomArchiveWarning)
                    {
                        shownCustomArchiveWarning = true;
                        PendingPopup = new YesNoPopupContents("This playlist uses mirror download links. Would you like to use them?", () => SetCustomArchivePreference(true),
                             noButtonPressedCallback: () => SetCustomArchivePreference(false), animateParentCanvas: false);

                        await popupSemaphore.WaitAsync();
                        PendingPopup = null;

                        if (!preferCustomArchiveURL)
                        {
                            i--;
                            continue;
                        }
                    }
                    await BeatmapDownloadByCustomURL(customArchiveURL, identifier, downloadQueueEntry.cancellationTokenSource.Token, downloadQueueEntry);
                }
                else if (!string.IsNullOrEmpty(missingSongs[i].Hash))
                {
                    await BeatmapDownloadByHash(missingSongs[i].Hash, downloadQueueEntry.cancellationTokenSource.Token, downloadQueueEntry);
                }
                else if (!string.IsNullOrEmpty(missingSongs[i].Key))
                {
                    var hash = await BeatmapDownloadByKey(missingSongs[i].Key.ToLower(), downloadQueueEntry.cancellationTokenSource.Token, downloadQueueEntry);
                    if (!string.IsNullOrEmpty(hash))
                    {
                        missingSongs[i].Hash = hash;
                    }
                }

                downloadQueueEntry.SetTotalProgress(i + 1);

                if (downloadQueueEntry.Aborted)
                {
                    break;
                }

                if (disposed)
                {
                    // If downloader is disposed, a soft restart is happening. Reinstantiate the cancellation token and leave.
                    downloadQueueEntry.cancellationTokenSource = new CancellationTokenSource();
                    return;
                }

                if (downloadQueueEntry.cancellationTokenSource.IsCancellationRequested)
                {
                    // If we directly cancel, it is a pause. So we wait at this semaphore till it is released.
                    await pauseSemaphore.WaitAsync();
                    i--;
                    downloadQueueEntry.cancellationTokenSource = new CancellationTokenSource();
                }
            }

            downloadQueueEntry.parentManager.StorePlaylist(downloadQueueEntry.playlist);

            if (downloadQueueEntry.playlist is BeatSaberPlaylistsLib.Types.Playlist playlist)
            {
                coversToRefresh.AddLast(playlist);
            }
            
            downloadQueueEntry.DownloadAbortedEvent -= OnDownloadAborted;
            currentDownload = null;
        }

        private void SetCustomArchivePreference(bool preferCustomArchiveURL)
        {
            this.preferCustomArchiveURL = preferCustomArchiveURL;
            popupSemaphore.Release();
        }

        #region Map Download

        private async Task BeatSaverBeatmapDownload(Beatmap song, BeatmapVersion songversion, CancellationToken token, IProgress<double> progress = null)
        {
            var customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
            if (!Directory.Exists(customSongsPath))
            {
                Directory.CreateDirectory(customSongsPath);
            }

            if (!ownedHashes.Contains(songversion.Hash.ToUpper()))
            {
                var zip = await songversion.DownloadZIP(token, progress).ConfigureAwait(false);
                await ExtractZipAsync(zip, customSongsPath, FolderNameForBeatsaverMap(song)).ConfigureAwait(false);
                ownedHashes.Add(songversion.Hash.ToUpper());
            }
        }

        private async Task<string> BeatmapDownloadByKey(string key, CancellationToken token, IProgress<double> progress = null)
        {
            if (!token.IsCancellationRequested)
            {
                try
                {
                    var song = await beatSaverInstance.Beatmap(key, token);
                    if (song == null)
                    {
                        Plugin.Log.Error($"Failed to download Song {key}. Unable to find a beatmap for that hash.");
                        return "";
                    }
                    // A key is not enough to identify a specific version. So just get the latest one.
                    if (Loader.GetLevelByHash(song.LatestVersion.Hash) == null)
                    {
                        await BeatSaverBeatmapDownload(song, song.LatestVersion, token, progress);
                    }
                    return song.LatestVersion.Hash;   
                }
                catch (Exception e)
                {
                    if (e is not TaskCanceledException)
                    {
                        Plugin.Log.Error($"Failed to download Song {key}. Exception: {e}");
                    }
                }   
            }
            return "";
        }

        private async Task BeatmapDownloadByHash(string hash, CancellationToken token, IProgress<double> progress = null)
        {
            if (!token.IsCancellationRequested)
            {
                try
                {
                    var song = await beatSaverInstance.BeatmapByHash(hash, token);
                    if (song == null)
                    {
                        Plugin.Log.Error($"Failed to download Song {hash}. Unable to find a beatmap for that hash.");
                        return;
                    }

                    BeatmapVersion matchingVersion = null;
                    foreach (var version in song.Versions)
                    {
                        if (string.Equals(hash, version.Hash, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingVersion = version;
                        }
                    }

                    if (matchingVersion != null)
                    {
                        await BeatSaverBeatmapDownload(song, matchingVersion, token, progress);
                    }
                    else
                    {
                        await BeatmapDownloadByCustomURL($"https://cdn.beatsaver.com/{hash.ToLowerInvariant()}.zip", FolderNameForBeatsaverMap(song), token, progress as IProgress<float>);
                    }
                }
                catch (Exception e)
                {
                    if (e is not TaskCanceledException)
                    {
                        Plugin.Log.Error($"Failed to download Song {hash}. Exception: {e}");
                    }
                }
            }
        }

        private async Task BeatmapDownloadByCustomURL(string url, string songName, CancellationToken token, IProgress<float> progress = null)
        {
            if (!token.IsCancellationRequested)
            {
                try
                {
                    var customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
                    if (!Directory.Exists(customSongsPath))
                    {
                        Directory.CreateDirectory(customSongsPath);
                    }
                    var httpResponse = await siraHttpService.GetAsync(url, progress, token);
                    if (httpResponse.Successful)
                    {
                        var zip = await httpResponse.ReadAsByteArrayAsync();
                        await ExtractZipAsync(zip, customSongsPath, songName).ConfigureAwait(false);
                    }
                    else
                    {
                        Plugin.Log.Error($"Failed to download Song {url}");
                    }
                }
                catch (Exception e)
                {
                    if (e is not TaskCanceledException)
                    {
                        Plugin.Log.Error($"Failed to download Song {url}");
                    }
                }   
            }
        }

        private string FolderNameForBeatsaverMap(Beatmap song)
        {
            // A workaround for the max path issue and long folder names
            var longFolderName = song.ID + " (" + song.Metadata.LevelAuthorName + " - " + song.Metadata.SongName;
            return longFolderName.Truncate(49, true) + ")";
        }

        private async Task ExtractZipAsync(byte[] zip, string customSongsPath, string songName, bool overwrite = false)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                var basePath = "";
                basePath = string.Join("", songName.Split(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()));
                var path = Path.Combine(customSongsPath, basePath);

                if (!overwrite && Directory.Exists(path))
                {
                    var pathNum = 1;
                    while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                    path += $" ({pathNum})";
                }

                if (PluginConfig.Instance.DriveFullProtection)
                {
                    var driveInfo = new DriveInfo(Path.GetPathRoot(path));

                    long totalSize = 0;
                    foreach (var entry in archive.Entries)
                    {
                        totalSize += entry.Length;
                    }

                    if (driveInfo.AvailableFreeSpace - totalSize < 104857600 && !ignoredDiskWarning) // If less than 100MB
                    {
                        CreateDrivePopup();

                        await popupSemaphore.WaitAsync();
                        PendingPopup = null;

                        if (!ignoredDiskWarning)
                        {
                            currentDownload.AbortDownload();
                            downloadQueue.Clear();
                            downloadQueue.Add(currentDownload); // Add it back because we remove the first element of queue after a download
                            return;
                        }
                    }
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Name) && entry.Name == entry.FullName)
                        {
                            var entryPath = Path.Combine(path, entry.Name); // Name instead of FullName for better security and because song zips don't have nested directories anyway
                            if (overwrite || !File.Exists(entryPath)) // Either we're overwriting or there's no existing file
                                entry.ExtractToFile(entryPath, overwrite);
                        }
                    }
                }).ConfigureAwait(false);
                archive.Dispose();
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"Unable to extract ZIP! Exception: {e}");
                return;
            }
            zipStream.Close();
        }

        private void CreateDrivePopup()
        {
            var popupText = "You are running out of disk space (less than 100MB), continuing the download can cause issues such as corrupt game configs" +
                            " (as there may not be enough space to save them).";

            if (PluginConfig.Instance.EasterEggs && PluginConfig.Instance.AuthorName.ToUpper().Contains("SKALX"))
            {
                popupText = "Remember the October 26th, 2021 \"JoeSaber\" incident? Wanna do it again?";
            }

            PendingPopup = new YesNoPopupContents(popupText, yesButtonText: "Continue", noButtonText: "Abort", yesButtonPressedCallback: () =>
            {
                ignoredDiskWarning = true;
                popupSemaphore.Release();
            },
            noButtonPressedCallback: () =>
            {
                ignoredDiskWarning = false;
                popupSemaphore.Release();
            }, animateParentCanvas: false);
        }

        #endregion
    }
}
