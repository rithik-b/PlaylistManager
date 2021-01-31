using IPA.Utilities;
using System;
using System.Threading.Tasks;

namespace PlaylistManager.Utilities
{
    class UserInfoUtils
    {
        private static FieldAccessor<PlatformLeaderboardsModel, IPlatformUserModel>.Accessor AccessPlatformUserModel;
        private static IPlatformUserModel platformUserModel;
        private static UserInfo userInfo;

        internal static event Action PlatformUserModelLoadedEvent;
        internal static bool HasPlatformUserModelLoaded = false;

        UserInfoUtils(PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            UserInfoUtils.platformUserModel = AccessPlatformUserModel(ref platformLeaderboardsModel);
            UserInfoUtils.HasPlatformUserModelLoaded = true;
            UserInfoUtils.PlatformUserModelLoadedEvent?.Invoke();
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
