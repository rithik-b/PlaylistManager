using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;
using PlaylistManager.UI;
using PlaylistManager.Managers;

namespace PlaylistManager.Installers
{
    class PlaylistViewInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<AddPlaylistController>().AsSingle();
            Container.BindInterfacesTo<RemoveFromPlaylistController>().AsSingle();
            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();
            Container.BindInterfacesTo<PlaylistViewController>().AsSingle();
        }
    }
}
