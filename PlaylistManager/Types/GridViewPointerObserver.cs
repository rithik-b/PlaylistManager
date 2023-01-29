using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlaylistManager.Types
{
    public class GridViewPointerObserver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GridScrollView? gridScrollView;

        private void Awake()
        {
            gridScrollView = GetComponent<GridScrollView>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (gridScrollView != null) 
                gridScrollView.OnHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (gridScrollView != null) 
                gridScrollView.OnLeave();
        }
    }
}