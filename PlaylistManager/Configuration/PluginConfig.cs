using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IPA.Config.Stores;
using PlaylistManager.Utilities;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PlaylistManager.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual string AuthorName { get; set; }
        public virtual bool DefaultImageDisabled { get; set; } = false;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            if (AuthorName == null)
            {
                if (!UserInfoUtils.HasPlatformUserModelLoaded)
                {
                    UserInfoUtils.PlatformUserModelLoadedEvent += SetAuthorName;
                }
                else
                {
                    SetAuthorName();
                }
            }
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }

        private void SetAuthorName()
        {
            Task.Run(() => SetAuthorNameAsync());
            UserInfoUtils.PlatformUserModelLoadedEvent -= SetAuthorName;
        }

        private async void SetAuthorNameAsync()
        {
            AuthorName = "PlaylistManager";
            Plugin.Log.Info("Finding UserInfo");
            UserInfo userInfo = await UserInfoUtils.GetUserInfoAsync();
            Plugin.Log.Info("UserInfo found");
            if (userInfo != null)
            {
                AuthorName = userInfo.userName;
                Plugin.Log.Info("UserInfo not null");
            }
        }
    }
}
