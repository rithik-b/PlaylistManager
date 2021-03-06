﻿using Zenject;
using PlaylistManager.UI;
using PlaylistManager.Managers;
using PlaylistManager.Configuration;

namespace PlaylistManager.Installers
{
    class PlaylistManagerMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!PluginConfig.Instance.ManagementDisabled)
            {
                Container.BindInterfacesTo<LevelDetailButtonsViewController>().AsSingle();
                Container.BindInterfacesAndSelfTo<AddPlaylistModalController>().AsSingle();
                Container.BindInterfacesTo<PlaylistViewButtonsController>().AsSingle();
                Container.BindInterfacesAndSelfTo<PlaylistDetailsViewController>().AsSingle();
                Container.BindInterfacesAndSelfTo<ImageSelectionModalController>().AsSingle();
                Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();
            }

            if (PluginConfig.Instance.FoldersDisabled || PluginConfig.Instance.ManagementDisabled)
            {
                Container.BindInterfacesTo<AllPacksRefresher>().AsSingle();
            }
            else
            {
                Container.BindInterfacesTo<FoldersViewController>().AsSingle();
            }


            Container.BindInterfacesTo<TableViewButtonsController>().AsSingle();
            Container.BindInterfacesTo<CoverImageUpdater>().AsSingle();
            Container.BindInterfacesAndSelfTo<DifficultyHighlighter>().AsSingle();

            Container.BindInterfacesTo<SettingsViewController>().AsSingle();
            Container.BindInterfacesTo<RefreshButtonUI>().AsSingle();

            Container.BindInterfacesTo<PlaylistUIManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistDataManager>().AsSingle();
        }
    }
}
