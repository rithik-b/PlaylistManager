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
        private string userAgent;
        public static DownloaderUtils instance;
        public static void Init()
        {
            instance = new DownloaderUtils();
            HttpOptions options = new HttpOptions(name: typeof(DownloaderUtils).Assembly.GetName().Name, version: typeof(DownloaderUtils).Assembly.GetName().Version);
            instance.beatSaverInstance = new BeatSaver(options);
            instance.userAgent = string.Format("{0}/{1} (+https://github.com/rithik-b/PlaylistManager)", typeof(DownloaderUtils).Assembly.GetName().Name, typeof(DownloaderUtils).Assembly.GetName().Version);
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
            var song = await beatSaverInstance.Key(key, options);
            try
            {
                await BeatSaverBeatmapDownload(song, options, direct);
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException))
                    Plugin.Log.Critical(string.Format("Failed to download Song {0}", key));
            }
        }

        public async Task BeatmapDownloadByHash(string hash, CancellationToken token, IProgress<double> progress = null, bool direct = false)
        {
            var options = new StandardRequestOptions { Token = token, Progress = progress };
            var song = await beatSaverInstance.Hash(hash, options);
            try
            {
                await BeatSaverBeatmapDownload(song, options, direct);
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException))
                    Plugin.Log.Critical(string.Format("Failed to download Song {0}", hash));
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
                var request = WebRequest.CreateHttp(url);
                Plugin.Log.Critical(url);
                request.UserAgent = userAgent;
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    var zip = ms.ToArray();
                    await ExtractZipAsync(zip, customSongsPath, songName: songName).ConfigureAwait(false);
                }
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
                string path = customSongsPath + "/" + basePath;
                
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
                        var entryPath = Path.Combine(path, entry.Name); // Name instead of FullName for better security and because song zips don't have nested directories anyway
                        if (overwrite || !File.Exists(entryPath)) // Either we're overwriting or there's no existing file
                            entry.ExtractToFile(entryPath, overwrite);
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

        public async Task<Stream> DownloadFileToStreamAsync(string url, CancellationToken token)
        {
            Uri uri = new Uri(url);
            using (var webClient = new WebClient())
            using (var registration = token.Register(() => webClient.CancelAsync()))
            {
                var data = await webClient.DownloadDataTaskAsync(uri);
                return new MemoryStream(data);
            }
        }
    }
}
