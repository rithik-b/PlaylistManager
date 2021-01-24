using BeatSaverSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/*
 * Original Author: KyleMC1413
 * Adapted from BeatSaverDownloader
 */

namespace PlaylistManager.Utilities
{
    class DownloaderUtils
    {
        private BeatSaver beatSaverInstance;
        public static DownloaderUtils instance;

        public static void Init()
        {
            instance = new DownloaderUtils();
            HttpOptions options = new HttpOptions
            {
                ApplicationName = typeof(DownloaderUtils).Assembly.GetName().Name,
                Version = typeof(DownloaderUtils).Assembly.GetName().Version,
            };
            instance.beatSaverInstance = new BeatSaver(options);
        }

        public async Task BeatmapDownloadByKey(string key, CancellationToken token, IProgress<double> progress = null, bool direct = false)
        {
            var song = await beatSaverInstance.Key(key, token, progress);
            try
            {
                string customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
                if (!Directory.Exists(customSongsPath))
                {
                    Directory.CreateDirectory(customSongsPath);
                }
                var zip = await song.DownloadZip(direct, token, progress).ConfigureAwait(false);
                await ExtractZipAsync(song, zip, customSongsPath).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    Plugin.Log.Warn("Song Download Aborted.");
                else
                    Plugin.Log.Critical("Failed to download Song!");
            }
        }

        public async Task BeatmapDownloadByHash(string hash, CancellationToken token, IProgress<double> progress = null, bool direct = false)
        {
            var song = await beatSaverInstance.Hash(hash, token, progress);
            try
            {
                string customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
                if (!Directory.Exists(customSongsPath))
                {
                    Directory.CreateDirectory(customSongsPath);
                }
                var zip = await song.DownloadZip(direct, token, progress).ConfigureAwait(false);
                await ExtractZipAsync(song, zip, customSongsPath).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    Plugin.Log.Warn("Song Download Aborted.");
                else
                    Plugin.Log.Critical("Failed to download Song!");
            }
        }

        private async Task ExtractZipAsync(BeatSaverSharp.Beatmap songInfo, byte[] zip, string customSongsPath, bool overwrite = false)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                string basePath = songInfo.Key + " (" + songInfo.Metadata.SongName + " - " + songInfo.Metadata.LevelAuthorName + ")";
                basePath = string.Join("", basePath.Split((Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray())));
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
    }
}
