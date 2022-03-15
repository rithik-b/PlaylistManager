using System;
using PlaylistManager.AffinityPatches;
using PlaylistManager.Utilities;
using Zenject;

namespace PlaylistManager.Installers
{
    internal class PlaylistManagerAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlaylistDownloader>().AsSingle();
            
            Container.BindInterfacesTo<LevelCollectionCellSetDataPatch>().AsSingle();
            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            {
                Container.BindInterfacesTo<Amogus>().AsSingle();
            }
        }
    }
}
