using System;
using System.Threading.Tasks;

namespace PlaylistManager.Utilities
{
    class UserInfoUtils
    {
        private PlatformLeaderboardsModel platformLeaderboardsModel;

        private static IPlatformUserModel platformUserModel;
        private static UserInfo userInfo;

        internal static event Action PlatformUserModelLoadedEvent;
        internal static bool HasPlatformUserModelLoaded = false;

        UserInfoUtils(IPlatformUserModel platformUserModel)
        {
            UserInfoUtils.platformUserModel = platformUserModel;
            HasPlatformUserModelLoaded = true;
            PlatformUserModelLoadedEvent?.Invoke();
        }

        internal static async Task<UserInfo> GetUserInfoAsync()
        {
            if (userInfo == null)
            {
                userInfo = await platformUserModel.GetUserInfo();
            }
            return userInfo;
        }
    }
}
