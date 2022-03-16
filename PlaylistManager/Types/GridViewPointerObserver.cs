using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlaylistManager.Types
{
    public class GridViewPointerObserver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GridView gridView;
        private GridScrollView gridScrollView;

        private void Awake()
        {
            gridView = GetComponent<GridView>();
            gridScrollView = GetComponent<GridScrollView>();
        }

        public void OnPointerEnter(PointerEventData eventData) => gridScrollView.OnHover();

        public void OnPointerExit(PointerEventData eventData) => gridScrollView.OnLeave();
    }
}