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

		private float zeroPos;
		private float endPos;

		private float contentSize => _contentRectTransform.rect.height;
		private float currentPos => _contentRectTransform.localPosition.y;

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

			zeroPos = -(contentSize - 15) / 2;
			endPos = -zeroPos - (_fixedCellSize * 4);
			_destinationPos = annotatedBeatmapLevelCollectionsGridViewAnimator.GetContentYOffset();
			RefreshButtons();
		}

		public void OnLeave()
        {
			_destinationPos = annotatedBeatmapLevelCollectionsGridViewAnimator.GetContentYOffset();
			enabled = false;
        }

		public override void UpdateContentSize()
		{
			SetContentSize(contentSize);
			bool active = contentSize - (_fixedCellSize * 3) > 0f;

			_pageUpButton.gameObject.SetActive(active); 
			_pageDownButton.gameObject.SetActive(active);
			_verticalScrollIndicator.gameObject.SetActive(active);
		}

		public override void RefreshButtons()
		{
			if (_pageUpButton != null)
			{
				_pageUpButton.interactable = (_destinationPos > zeroPos + 0.001f);
			}
			if (_pageDownButton != null)
			{
				_pageDownButton.interactable = (_destinationPos < endPos - 0.001f);
			}
		}

        public override void SetDestinationPos(float value)
        {
			float difference = contentSize - (_fixedCellSize * 4);
            if (difference <= 0f)
            {
                _destinationPos = zeroPos;
                return;
            }
			_destinationPos = Mathf.Clamp(value, zeroPos, endPos);
		}

		// We ignore the position given here since it is normalized
		public override void UpdateVerticalScrollIndicator(float _)
        {
			if (_verticalScrollIndicator != null)
			{
				// To show a blank progress bar, we do NaN
				if (contentSize / _fixedCellSize <= 5)
                {
					_verticalScrollIndicator.progress = float.NaN;
					return;
                }
				_verticalScrollIndicator.progress = (currentPos - zeroPos) / (endPos - zeroPos);
			}
		}
    }
}
