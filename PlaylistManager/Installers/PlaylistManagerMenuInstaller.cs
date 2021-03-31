using Zenject;
using PlaylistManager.UI;
using PlaylistManager.Managers;

namespace PlaylistManager.Installers
{
    class PlaylistManagerMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<AddRemoveButtonsViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddPlaylistModalController>().AsSingle();
            Container.BindInterfacesTo<PlaylistViewButtonsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistDetailsViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImageSelectionModalController>().AsSingle();
            Container.BindInterfacesTo<FoldersViewController>().AsSingle();
            Container.BindInterfacesTo<TableViewButtonsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();
            Container.BindInterfacesTo<SettingsViewController>().AsSingle();

            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistDataManager>().AsSingle();

            Container.BindInterfacesTo<DifficultyHighlighter>().AsSingle();
            Container.BindInterfacesTo<RefreshButtonUI>().AsSingle();
        }
    }
}
