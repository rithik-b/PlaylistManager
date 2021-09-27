using HMUI;
using UnityEngine;

namespace PlaylistManager.Utilities
{
    internal class Utils
    {
        public static void TransferScrollBar(ScrollView sender, ScrollView reciever)
        {
            Accessors.PageUpAccessor(ref reciever) = Accessors.PageUpAccessor(ref sender);
            Accessors.PageDownAccessor(ref reciever) = Accessors.PageDownAccessor(ref sender);
            Accessors.ScrollIndicatorAccessor(ref reciever) = Accessors.ScrollIndicatorAccessor(ref sender);

            RectTransform scrollBar = sender.transform.Find("ScrollBar").GetComponent<RectTransform>();
            scrollBar.SetParent(sender.transform.parent);
            GameObject.Destroy(sender.gameObject);
            scrollBar.sizeDelta = new Vector2(8f, scrollBar.sizeDelta.y);
        }
    }
}
