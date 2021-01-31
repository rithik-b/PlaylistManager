using Zenject;
using PlaylistManager.UI;
using PlaylistManager.Managers;

namespace PlaylistManager.Installers
{
    class PlaylistViewInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<AddPlaylistController>().AsSingle();
            Container.BindInterfacesAndSelfTo<RemoveFromPlaylistController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistViewController>().AsSingle();
            Container.BindInterfacesTo<ButtonViewController>().AsSingle();
            Container.BindInterfacesTo<PlaylistViewButtonsController>().AsSingle();
            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();
        }
    }
}
