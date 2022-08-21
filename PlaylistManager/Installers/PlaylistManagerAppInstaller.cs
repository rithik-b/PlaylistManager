using System;
using PlaylistManager.AffinityPatches;
using PlaylistManager.Downloaders;
using PlaylistManager.Services;
using PlaylistManager.Utilities;
using Polyglot;
using Zenject;

namespace PlaylistManager.Installers
{
    internal class PlaylistManagerAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlaylistDownloader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlaylistSequentialDownloader>().AsSingle();
            Container.Bind<AuthorNameService>().AsSingle();
            Container.Bind<PlaylistCreationService>().AsSingle();

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1 && LocalizationProvider.Instance.localization.SelectedLanguage == Language.English)
            {
                Container.BindInterfacesTo<Amogus>().AsSingle();
            }
        }
    }
}
