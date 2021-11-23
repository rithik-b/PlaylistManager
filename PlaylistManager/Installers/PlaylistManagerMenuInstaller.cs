using SiraUtil;
using Zenject;
using PlaylistManager.UI;
using PlaylistManager.Managers;
using PlaylistManager.Configuration;

namespace PlaylistManager.Installers
{
    internal class PlaylistManagerMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<LevelDetailButtonsViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddPlaylistModalController>().AsSingle();
            Container.BindInterfacesTo<PlaylistDetailViewButtonsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistDetailsViewController>().AsSingle();
            Container.Bind<ImageSelectionModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();

            Container.BindInterfacesAndSelfTo<PlaylistDownloaderViewController>().AsSingle();
            Container.BindInterfacesTo<PlaylistViewButtonsController>().AsSingle();

            Container.BindInterfacesTo<PlaylistsGridViewController>().AsSingle();
            Container.BindInterfacesTo<CoverImageUpdater>().AsSingle();
            Container.BindInterfacesAndSelfTo<DifficultyHighlighter>().AsSingle();

            Container.Bind<SettingsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<RefreshButtonUI>().AsSingle();

            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistDataManager>().AsSingle();

            if (PluginConfig.Instance.FoldersDisabled)
            {
                Container.BindInterfacesTo<AllPacksRefresher>().AsSingle();
            }
            else
            {
                Container.BindInterfacesTo<FoldersViewController>().AsSingle();
            }
        }
    }
}
