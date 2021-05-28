using BeatSaverSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

/*
 * Original Author: KyleMC1413
 * Adapted from BeatSaverDownloader
 */

namespace PlaylistManager.Utilities
{
    internal class DownloaderUtils
    {
        private BeatSaver beatSaverInstance;
        public static DownloaderUtils instance;
        public static void Init()
        {
            instance = new DownloaderUtils();
            HttpOptions options = new HttpOptions(name: typeof(DownloaderUtils).Assembly.GetName().Name, version: typeof(DownloaderUtils).Assembly.GetName().Version);
            instance.beatSaverInstance = new BeatSaver(options);
        }

        private async Task BeatSaverBeatmapDownload(Beatmap song, StandardRequestOptions options, bool direct)
        {
            string customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
            if (!Directory.Exists(customSongsPath))
            {
                Directory.CreateDirectory(customSongsPath);
            }
            var zip = await song.ZipBytes(direct, options).ConfigureAwait(false);
            await ExtractZipAsync(zip, customSongsPath, songInfo: song).ConfigureAwait(false);
        }

        public async Task BeatmapDownloadByKey(string key, CancellationToken token, IProgress<double> progress = null, bool direct = false)
        {
            var options = new StandardRequestOptions { Token = token, Progress = progress };
            bool songDownloaded = false;
            while(!songDownloaded)
            { 
                try
                {
                    var song = await beatSaverInstance.Key(key, options);
                    await BeatSaverBeatmapDownload(song, options, direct);
                    songDownloaded = true;
                }
                catch (Exception e)
                {
                    if (e is BeatSaverSharp.Exceptions.RateLimitExceededException rateLimitException)
                    {
                        double timeRemaining = (rateLimitException.RateLimit.Reset - DateTime.Now).TotalMilliseconds;
                        timeRemaining = timeRemaining > 0 ? timeRemaining : 0;
                        await Task.Delay((int)timeRemaining);
                        continue;
                    }
                    else if (!(e is TaskCanceledException))
                    {
                        Plugin.Log.Critical(string.Format("Failed to download Song {0}. Exception: {1}", key, e.ToString()));
                    }
                    songDownloaded = true;
                }
            }
        }

        public async Task BeatmapDownloadByHash(string hash, CancellationToken token, IProgress<double> progress = null, bool direct = false)
        {
            var options = new StandardRequestOptions { Token = token, Progress = progress };
            bool songDownloaded = false;
            while (!songDownloaded)
            {
                try
                {
                    var song = await beatSaverInstance.Hash(hash, options);
                    await BeatSaverBeatmapDownload(song, options, direct);
                    songDownloaded = true;
                }
                catch (Exception e)
                {
                    if (e is BeatSaverSharp.Exceptions.RateLimitExceededException rateLimitException)
                    {
                        double timeRemaining = (rateLimitException.RateLimit.Reset - DateTime.Now).TotalMilliseconds;
                        timeRemaining = timeRemaining > 0 ? timeRemaining : 0;
                        await Task.Delay((int)timeRemaining);
                        continue;
                    }
                    else if (!(e is TaskCanceledException))
                    {
                        Plugin.Log.Critical(string.Format("Failed to download Song {0}. Exception: {1}", hash, e.ToString()));
                    }
                    songDownloaded = true;
                }
            }
        }

        public async Task BeatmapDownloadByCustomURL(string url, string songName, CancellationToken token)
        {
            try
            {
                string customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
                if (!Directory.Exists(customSongsPath))
                {
                    Directory.CreateDirectory(customSongsPath);
                }
                var zip = await DownloadFileToBytesAsync(url, token);
                await ExtractZipAsync(zip, customSongsPath, songName: songName).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException))
                    Plugin.Log.Critical(string.Format("Failed to download Song {0}", url));
            }
        }

        private async Task ExtractZipAsync(byte[] zip, string customSongsPath, bool overwrite = false, string songName = null, Beatmap songInfo = null)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                string basePath = "";
                if (songInfo != null)
                {
                    basePath = songInfo.Key + " (" + songInfo.Metadata.SongName + " - " + songInfo.Metadata.LevelAuthorName + ")";
                }
                else
                {
                    basePath = songName;
                }
                basePath = string.Join("", basePath.Split(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()));
                string path = Path.Combine(customSongsPath, basePath);
                
                if (!overwrite && Directory.Exists(path))
                {
                    int pathNum = 1;
                    while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                    path += $" ({pathNum})";
                }
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Name))
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
                Plugin.Log.Critical($"Unable to extract ZIP! Exception: {e}");
                return;
            }
            zipStream.Close();
        }

        public async Task<byte[]> DownloadFileToBytesAsync(string url, CancellationToken token)
        {
            Uri uri = new Uri(url);
            using (var webClient = new WebClient())
            using (var registration = token.Register(() => webClient.CancelAsync()))
            {
                var data = await webClient.DownloadDataTaskAsync(uri);
                return data;
            }
        }
    }
}
