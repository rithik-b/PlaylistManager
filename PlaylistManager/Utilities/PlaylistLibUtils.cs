using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Configuration;
using UnityEngine;

namespace PlaylistManager.Utilities
{
    public class PlaylistLibUtils
    {
        private static readonly string EASTER_EGG_URL = "https://raw.githubusercontent.com/rithik-b/PlaylistManager/master/img/easteregg.bplist";

        public static BeatSaberPlaylistsLib.PlaylistManager playlistManager
        {
            get
            {
                return BeatSaberPlaylistsLib.PlaylistManager.DefaultManager;
            }
        }

        public static BeatSaberPlaylistsLib.Types.IPlaylist CreatePlaylistWithConfig(string playlistName, BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            string playlistAuthorName = PluginConfig.Instance.AuthorName;
            bool easterEgg = playlistAuthorName.ToUpper().Contains("BINTER") && playlistName.ToUpper().Contains("TECH") && PluginConfig.Instance.EasterEggs;
            return CreatePlaylist(playlistName, playlistAuthorName, playlistManager, !PluginConfig.Instance.DefaultImageDisabled, PluginConfig.Instance.DefaultAllowDuplicates, easterEgg);
        }

        public static BeatSaberPlaylistsLib.Types.IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName, BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool defaultCover = true,
            bool allowDups = true, bool easterEgg = false)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistManager.CreatePlaylist("", playlistName, playlistAuthorName, "");

            if (defaultCover)
            {
                using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.Logo.png"))
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

        public static List<IPlaylistSong> GetMissingSongs(BeatSaberPlaylistsLib.Types.IPlaylist playlist, HashSet<string> ownedHashes = null)
        {
            if (playlist is LegacyPlaylist legacyPlaylist)
            {
                return legacyPlaylist.Where(s => s.PreviewBeatmapLevel == null && !(ownedHashes?.Contains(s.Hash) ?? false)).Distinct(IPlaylistSongComparer<IPlaylistSong>.Default).ToList();
            }
            else if (playlist is BlistPlaylist blistPlaylist)
            {
                return blistPlaylist.Where(s => s.PreviewBeatmapLevel == null && !(ownedHashes?.Contains(s.Hash) ?? false)).Distinct(IPlaylistSongComparer<IPlaylistSong>.Default).ToList();
            }
            else
            {
                return null;
            }
        }

        #region Image

        private static DrawSettings defaultDrawSettings = new DrawSettings
        {
            Color = System.Drawing.Color.White,
            DrawStyle = DrawStyle.Normal,
            Font = BeatSaberPlaylistsLib.Utilities.FindFont("Microsoft Sans Serif", 80, FontStyle.Regular),
            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            },
            MinTextSize = 80,
            MaxTextSize = 140,
            WrapWidth = 10
        };

        private static Stream GetFolderImageStream() =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.FolderIcon.png");

        internal static Sprite DrawFolderIcon(string str)
        {
            if (str.Length > 15)
            {
                str = str.Substring(0, 15) + "...";
            }
            Image img = ImageUtilities.DrawString("\n"+str, Image.FromStream(GetFolderImageStream()), defaultDrawSettings);
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            return BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(ms.ToArray());
        }

        internal static Sprite GeneratePlaylistIcon(BeatSaberPlaylistsLib.Types.IPlaylist playlist) => BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(BeatSaberPlaylistsLib.Utilities.GenerateCoverForPlaylist(playlist).ToArray());

        #endregion
    }
}
