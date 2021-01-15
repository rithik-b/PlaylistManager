using System;
using System.IO;
namespace PlaylistManager.Utilities
{
    class PlaylistLibUtils
    {
        internal static BeatSaberPlaylistsLib.PlaylistManager LibDefaultManager
        {
            get
            {
                return BeatSaberPlaylistsLib.PlaylistManager.DefaultManager;
            }
        }

        public static void CreatePlaylist(string playlistName, string playlistAuthorName)
        {
            // ToDo try handler path
            string playlistFolderPath = Path.Combine(Environment.CurrentDirectory, "Playlists");
            string playlistFileName = String.Join("_", playlistName.Split(' '));
            string playlistPath = Path.Combine(playlistFolderPath, playlistFileName + ".blist");
            string originalPlaylistPath = Path.Combine(playlistFolderPath, playlistFileName);
            int dupNum = 0;
            while (File.Exists(playlistPath))
            {
                dupNum++;
                playlistPath = originalPlaylistPath + string.Format("({0}).blist", dupNum);
            }

            LibDefaultManager.CreatePlaylist(playlistPath, playlistName, playlistAuthorName, "");
        }
    }
}
