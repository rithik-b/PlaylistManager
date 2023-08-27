using HMUI;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistManager.Utilities
{
    internal class Accessors
    {
        #region ScrollView

        public static readonly FieldAccessor<ScrollView, IVRPlatformHelper>.Accessor PlatformHelperAccessor =
            FieldAccessor<ScrollView, IVRPlatformHelper>.GetAccessor("_platformHelper");

        #endregion

        #region Other

        public static readonly FieldAccessor<HoverHint, HoverHintController>.Accessor HoverHintControllerAccessor = FieldAccessor<HoverHint, HoverHintController>.GetAccessor("_hoverHintController");

        #endregion
    }
}
