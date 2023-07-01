using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using JetBrains.Annotations;
using PlaylistManager.Configuration;
using UnityEngine;

namespace PlaylistManager.Utilities
{
    public class PlaylistLibUtils
    {
        private const string ICON_PATH = "PlaylistManager.Icons.DefaultIcon.png";
        private const string EASTER_EGG_URL = "https://raw.githubusercontent.com/rithik-b/PlaylistManager/master/img/easteregg.bplist";

        public static BeatSaberPlaylistsLib.PlaylistManager playlistManager
        {
            get
            {
                return BeatSaberPlaylistsLib.PlaylistManager.DefaultManager;
            }
        }

        public static ICoroutineStarter coroutineStarter { get; set; }

        public static IPlaylist CreatePlaylistWithConfig(string playlistName, BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            var playlistAuthorName = PluginConfig.Instance.AuthorName;
            var easterEgg = playlistAuthorName.IndexOf("BINTER", StringComparison.OrdinalIgnoreCase) >= 0 && playlistName.IndexOf("TECH", StringComparison.OrdinalIgnoreCase) >= 0 && PluginConfig.Instance.EasterEggs;
            return CreatePlaylist(playlistName, playlistAuthorName, playlistManager, !PluginConfig.Instance.DefaultImageDisabled, PluginConfig.Instance.DefaultAllowDuplicates, easterEgg);
        }

        public static IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName, BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool defaultCover = true,
            bool allowDups = true, bool easterEgg = false)
        {
            var playlist = playlistManager.CreatePlaylist("", playlistName, playlistAuthorName, "");

            if (defaultCover)
            {
                using (var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ICON_PATH))
                {
                    playlist.SetCover(imageStream);
                }
            }


            if (!allowDups)
            {
                playlist.AllowDuplicates = false;
            }

            if (easterEgg)
            {
                playlist.SetCustomData("syncURL", EASTER_EGG_URL);
            }

            playlistManager.StorePlaylist(playlist);
            PlaylistLibUtils.playlistManager.RequestRefresh("PlaylistManager (plugin)");
            return playlist;
        }

        public static string GetIdentifierForPlaylistSong(IPlaylistSong playlistSong)
        {
            if (playlistSong.Identifiers.HasFlag(Identifier.Hash))
            {
                return playlistSong.Hash;
            }
            if (playlistSong.Identifiers.HasFlag(Identifier.Key))
            {
                return playlistSong.Key;
            }
            if (playlistSong.Identifiers.HasFlag(Identifier.LevelId))
            {
                return playlistSong.LevelId;
            }
            return "";
        }

        public static List<IPlaylistSong> GetMissingSongs(IPlaylist playlist, HashSet<string> ownedHashes = null)
        {
            if (playlist != null)
            {
                return playlist.Where(s => s.PreviewBeatmapLevel == null && !(ownedHashes?.Contains(s.Hash) ?? false)).Distinct(IPlaylistSongComparer<IPlaylistSong>.Default).ToList();
            }
            return new List<IPlaylistSong>();
        }

        public static IPlaylist[] TryGetAllPlaylists()
        {
            var playlists = playlistManager.GetAllPlaylists(true, out AggregateException ex);
            if (ex is not null)
            {
                Plugin.Log.Error(ex.Message);
                foreach (var e in ex.InnerExceptions)
                {
                    Plugin.Log.Error(e.ToString());
                }
            }

            return playlists;
        }

        #region Image


        private static Stream GetFolderImageStream() =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.FolderIcon.png");

        internal static async Task<Sprite> GeneratePlaylistIcon(IPlaylist playlist)
        {
            using var coverStream = await playlist.GetDefaultCoverStream();
            if (coverStream != null)
            {
                Sprite? sprite = null;
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(coverStream.ToArray()));
                return sprite ? sprite : BeatSaberPlaylistsLib.Utilities.DefaultSprite;
            }
            return BeatSaberPlaylistsLib.Utilities.DefaultSprite;
        }

        #endregion
    }
}
