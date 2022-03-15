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
        private PopupModalsController popupModalsController;
        private List<object> contributors;

        [UIComponent("contributors-list")]
        private readonly CustomCellListTableData customListTableData;

        [Inject]
        public void Contruct(PopupModalsController popupModalsController)
        {
            this.popupModalsController = popupModalsController;
        }

        public void Initialize()
        {
            contributors = new List<object>();
            contributors.Add(new Contributor("PixelBoom", "PlaylistManager", "PlaylistManager.Icons.Pixel.png", "https://www.youtube.com/channel/UCrk1WH6hCAdfrAtzv-q9hvQ",
                "https://www.twitch.tv/pixelboom58", "https://github.com/rithik-b", "https://ko-fi.com/pixelboom"));
            contributors.Add(new Contributor("Zingabopp", "BeatSaberPlaylistsLib", "PlaylistManager.Icons.Zinga.png", github: "https://github.com/Zingabopp", kofi: "https://ko-fi.com/zingabopp"));
            contributors.Add(new Contributor("Auros", "Major Contributor", "PlaylistManager.Icons.Auros.png", twitch: "https://www.twitch.tv/aurosvr", github: "https://github.com/Auros", kofi: "https://ko-fi.com/auros"));

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
