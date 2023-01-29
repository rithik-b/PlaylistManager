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
        public static string GetIdentifierForPlaylistSong(IPlaylistSong playlistSong)
        {
            if (playlistSong.Identifiers.HasFlag(Identifier.Hash))
            {
                return playlistSong.Hash!;
            }
            if (playlistSong.Identifiers.HasFlag(Identifier.Key))
            {
                return playlistSong.Key!;
            }
            if (playlistSong.Identifiers.HasFlag(Identifier.LevelId))
            {
                return playlistSong.LevelId!;
            }
            return "";
        }

        public static List<IPlaylistSong> GetMissingSongs(IPlaylist? playlist, HashSet<string>? ownedHashes = null)
        {
            if (playlist != null)
            {
                return playlist.Where(s => s.PreviewBeatmapLevel == null && !(ownedHashes?.Contains(s.Hash ?? "") ?? false)).Distinct(IPlaylistSongComparer<IPlaylistSong>.Default).ToList();
            }
            return new List<IPlaylistSong>();
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
