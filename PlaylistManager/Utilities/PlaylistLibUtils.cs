using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using BeatSaberPlaylistsLib;
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

        public static BeatSaberPlaylistsLib.Types.IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName, BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool defaultCover = true)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistManager.CreatePlaylist("", playlistName, playlistAuthorName, "");

            if (defaultCover)
            {
                using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.Logo.png"))
                {
                    playlist.SetCover(imageStream);
                }
            }

            if (!PluginConfig.Instance.DefaultAllowDuplicates)
            {
                playlist.AllowDuplicates = false;
            }

            // Easter Egg
            if (PluginConfig.Instance.AuthorName.ToUpper().Contains("BINTER") && playlistName.ToUpper().Contains("TECH") && PluginConfig.Instance.EasterEggs)
            {
                playlist.SetCustomData("syncURL", EASTER_EGG_URL);
            }

            playlistManager.StorePlaylist(playlist);
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
