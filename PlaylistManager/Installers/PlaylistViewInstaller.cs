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
            Container.BindInterfacesTo<PlaylistViewController>().AsSingle();
            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();
        }
    }
}
