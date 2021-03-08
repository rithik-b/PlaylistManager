using Zenject;
using PlaylistManager.UI;
using PlaylistManager.Managers;

namespace PlaylistManager.Installers
{
    class PlaylistViewInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AddPlaylistController>().AsSingle();
            Container.BindInterfacesAndSelfTo<RemoveFromPlaylistController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ButtonViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistViewButtonsController>().AsSingle();
            Container.BindInterfacesTo<FoldersViewController>().AsSingle();
            Container.BindInterfacesTo<TableViewButtonsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();
            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();

            Container.BindInterfacesTo<SettingsViewController>().AsSingle();
            Container.BindInterfacesTo<RefreshButtonUI>().AsSingle();
        }
    }
}
