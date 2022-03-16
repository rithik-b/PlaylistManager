using PlaylistManager.Configuration;
using PlaylistManager.Downloaders;
using Zenject;

namespace PlaylistManager.Installers
{
    internal class PlaylistManagerGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!PluginConfig.Instance.DownloadDuringGameplay)
            {
                Container.BindInterfacesTo<DownloadPauser>().AsSingle();
            }
        }
    }
}
