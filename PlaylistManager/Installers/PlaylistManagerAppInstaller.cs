using PlaylistManager.Downloaders;
using PlaylistManager.Utilities;
using Zenject;

namespace PlaylistManager.Installers
{
    internal class PlaylistManagerAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlaylistDownloader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistSequentialDownloader>().AsSingle();
        }
    }
}
