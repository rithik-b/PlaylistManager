using BeatSaverSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistManager.Utilities
{
    class DownloaderUtils
    {
        private BeatSaver beatSaverInstance;
        public static DownloaderUtils instance;
        private HashSet<string> alreadyDownloadedSongs;
        private bool extractingZip;

        public static void Init()
        {
            instance = new DownloaderUtils();
            instance.beatSaverInstance = new BeatSaver();
            instance.extractingZip = false;
        }

        private void SongLoader_SongsLoadedEvent(SongCore.Loader sender, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            alreadyDownloadedSongs = new HashSet<string>(levels.Values.Select(x => SongCore.Collections.hashForLevelID(x.levelID)));
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
                if (alreadyDownloadedSongs.Contains(song.Hash.ToUpper()))
                    alreadyDownloadedSongs.Remove(song.Hash.ToUpper());
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
                if (alreadyDownloadedSongs.Contains(song.Hash.ToUpper()))
                    alreadyDownloadedSongs.Remove(song.Hash.ToUpper());
            }
        }

        private async Task ExtractZipAsync(BeatSaverSharp.Beatmap songInfo, byte[] zip, string customSongsPath, bool overwrite = false)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                extractingZip = true;
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
                extractingZip = false;
                return;
            }
            zipStream.Close();
        }
    }
}
