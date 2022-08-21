using System.Reflection;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Configuration;

namespace PlaylistManager.Services
{
    internal class PlaylistCreationService
    {
        private const string kCoverPath = "PlaylistManager.Icons.DefaultCover.png";
        private const string kEasterEggURL = "https://raw.githubusercontent.com/rithik-b/PlaylistManager/master/img/easteregg.bplist";
        
        private readonly AuthorNameService authorNameService;

        public PlaylistCreationService(AuthorNameService authorNameService)
        {
            this.authorNameService = authorNameService;
        }
        
        public async Task<IPlaylist> CreatePlaylistAsync(string playlistName, BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            var playlistAuthorName = await authorNameService.GetNameAsync();
            var easterEgg = playlistAuthorName.ToUpper().Contains("BINTER") && playlistName.ToUpper().Contains("TECH") && PluginConfig.Instance.EasterEggs;
            return CreatePlaylist(playlistName, playlistAuthorName, playlistManager, !PluginConfig.Instance.DefaultImageDisabled, PluginConfig.Instance.DefaultAllowDuplicates, easterEgg);
        }

        private static IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName,
            BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool defaultCover = true,
            bool allowDups = true, bool easterEgg = false)
        {
            var playlist = playlistManager.CreatePlaylist("", playlistName, playlistAuthorName, "");

            if (defaultCover)
            {
                using var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(kCoverPath);
                playlist.SetCover(imageStream!);
            }


            if (!allowDups)
            {
                playlist.AllowDuplicates = false;
            }

            if (easterEgg)
            {
                playlist.SetCustomData("syncURL", kEasterEggURL);
            }

            playlistManager.StorePlaylist(playlist);
            BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.RequestRefresh("PlaylistManager (plugin)");
            return playlist;
        }
    }
}