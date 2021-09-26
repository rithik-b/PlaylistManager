using HMUI;
using PlaylistManager.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistManager.Types
{
    internal class GridScrollView : ScrollView
    {
		private AnnotatedBeatmapLevelCollectionsGridView annotatedBeatmapLevelCollectionsGridView;
		private AnnotatedBeatmapLevelCollectionsGridViewAnimator annotatedBeatmapLevelCollectionsGridViewAnimator;

		private float contentSize => _contentRectTransform.rect.height;
		private float zeroPos => -(contentSize - 15) / 2;

		public void Init(RectTransform viewport, RectTransform contentRectTransform, Button pageUpButton, Button pageDownButton, VerticalScrollIndicator verticalScrollIndicator)
        {
			_viewport = viewport;
			_contentRectTransform = contentRectTransform;
			_pageUpButton = pageUpButton;
			_pageDownButton = pageDownButton;
			_verticalScrollIndicator = verticalScrollIndicator;
			annotatedBeatmapLevelCollectionsGridView = GetComponent<AnnotatedBeatmapLevelCollectionsGridView>();
			annotatedBeatmapLevelCollectionsGridViewAnimator = GetComponent<AnnotatedBeatmapLevelCollectionsGridViewAnimator>();

			_scrollType = ScrollType.FixedCellSize;
			_fixedCellSize = annotatedBeatmapLevelCollectionsGridView.GetCellHeight();
			_joystickScrollSpeed = 30f * PluginConfig.Instance.PlaylistScrollSpeed;
		}

		public void OnHover()
        {
			enabled = true;
			UpdateContentSize();
            _destinationPos = annotatedBeatmapLevelCollectionsGridViewAnimator.GetContentYOffset();
		}

		public void OnLeave()
        {
			_destinationPos = annotatedBeatmapLevelCollectionsGridViewAnimator.GetContentYOffset();
			enabled = false;
        }

		public override void UpdateContentSize() => SetContentSize(contentSize);

		public override void RefreshButtons()
		{
			if (_pageUpButton != null)
			{
				_pageUpButton.interactable = (_destinationPos > zeroPos + 0.001f);
			}
			if (_pageDownButton != null)
			{
				_pageDownButton.interactable = (_destinationPos < -zeroPos - 0.001f);
			}
		}

        public override void SetDestinationPos(float value)
        {
            float difference = contentSize - _fixedCellSize;
            if (difference < 0f)
            {
                _destinationPos = zeroPos;
                return;
            }
            _destinationPos = Mathf.Clamp(value, zeroPos, -zeroPos - 60);
        }

        public override void UpdateVerticalScrollIndicator(float posY)
        {
			if (_verticalScrollIndicator != null)
			{
				_verticalScrollIndicator.progress = (posY - zeroPos) / contentSize;
			}
		}
    }
}
