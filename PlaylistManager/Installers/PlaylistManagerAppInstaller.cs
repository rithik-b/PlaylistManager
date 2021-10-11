using IPA.Loader;
using PlaylistManager.Utilities;
using Zenject;

namespace PlaylistManager.Installers
{
    internal class PlaylistManagerAppInstaller : Installer
    {
        private readonly PluginMetadata metadata;

        public PlaylistManagerAppInstaller(PluginMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(metadata).WithId(nameof(PlaylistManager)).AsCached();
            Container.Bind<PlaylistDownloader>().AsSingle();
        }
    }
}
