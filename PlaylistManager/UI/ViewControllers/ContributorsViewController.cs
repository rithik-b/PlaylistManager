using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using PlaylistManager.Types;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\ContributorsView.bsml")]
    [ViewDefinition("PlaylistManager.UI.Views.ContributorsView.bsml")]
    internal class ContributorsViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        [Inject]
        private readonly PopupModalsController popupModalsController = null!;
        
        [UIComponent("contributors-list")]
        private readonly CustomCellListTableData customListTableData;
        
        private readonly List<object> contributors = new()
        {
            new Contributor("PixelBoom", "PlaylistManager (PC)", "PlaylistManager.Icons.Pixel.png", "https://www.youtube.com/channel/UCrk1WH6hCAdfrAtzv-q9hvQ",
                "https://www.twitch.tv/pixelboom58", "https://github.com/rithik-b", "https://ko-fi.com/pixelboom"),
            new Contributor("Metalit", "PlaylistManager (Quest)", "PlaylistManager.Icons.Metalit.png", github: "https://github.com/Metalit"),
            new Contributor("Zingabopp", "BeatSaberPlaylistsLib (PC)", "PlaylistManager.Icons.Zinga.png", github: "https://github.com/Zingabopp", kofi: "https://ko-fi.com/zingabopp"),
            new Contributor("Auros", "Major Contributor (PC)", "PlaylistManager.Icons.Auros.png", twitch: "https://www.twitch.tv/aurosvr", github: "https://github.com/Auros", kofi: "https://ko-fi.com/auros")
        };

        public void Initialize()
        {
            foreach (var contributor in contributors.OfType<Contributor>())
            {
                contributor.OpenURL += URLRequested;
            }
        }

        public void Dispose()
        {
            foreach (var contributor in contributors.OfType<Contributor>())
            {
                contributor.OpenURL -= URLRequested;
            }
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            customListTableData.data.Clear();
            customListTableData.data = contributors;
            customListTableData.tableView.ReloadData();
        }

        private void URLRequested(string url)
        {
            popupModalsController.ShowYesNoModal(transform, $"Would you like to open\n{url}", () =>
            {
                Application.OpenURL(url);
            });
        }
    }
}
