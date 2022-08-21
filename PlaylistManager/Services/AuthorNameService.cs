using System.Threading.Tasks;
using PlaylistManager.Configuration;

namespace PlaylistManager.Services
{
    internal class AuthorNameService
    {
        private readonly IPlatformUserModel platformUserModel;

        public AuthorNameService(IPlatformUserModel platformUserModel)
        {
            this.platformUserModel = platformUserModel;
        }

        public async Task<string> GetNameAsync()
        {
            if (PluginConfig.Instance.AutomaticAuthorName)
            {
                var userInfo = await platformUserModel.GetUserInfo();
                return userInfo.userName;
            }

            return PluginConfig.Instance.AuthorName;
        }
    }
}