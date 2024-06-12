using SiraUtil.Affinity;
using UnityEngine;

namespace PlaylistManager.AffinityPatches
{
    public class AnnotatedBeatmapLevelCollectionsGridViewAnimatorPatches : IAffinity
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController _annotatedBeatmapLevelCollectionsTableViewController;
        private readonly SelectLevelCategoryViewController _selectLevelCategoryViewController;

        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory;
        private int _initialColumnCount;
        private int _initialVisibleColumnCount;

        public AnnotatedBeatmapLevelCollectionsGridViewAnimatorPatches(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController, SelectLevelCategoryViewController selectLevelCategoryViewController)
        {
            _annotatedBeatmapLevelCollectionsTableViewController = annotatedBeatmapLevelCollectionsTableViewController;
            _selectLevelCategoryViewController = selectLevelCategoryViewController;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.SetData))]
        [AffinityPrefix]
        private void AddColumnsAndResize(AnnotatedBeatmapLevelCollectionsGridView __instance)
        {
            Plugin.Log.Notice($"Column count: {__instance._gridView._columnCount} Row count: {__instance._gridView._rowCount}");

            var selectedLevelCategory = _selectLevelCategoryViewController.selectedLevelCategory;
            if (_lastSelectedLevelCategory == selectedLevelCategory)
            {
                return;
            }

            // Not sure why this could be called with values of 0.
            var animator = _annotatedBeatmapLevelCollectionsTableViewController._annotatedBeatmapLevelCollectionsGridView._animator;
            if (animator._columnCount == 0 || animator._visibleColumnCount == 0)
            {
                return;
            }

            if (_initialColumnCount == 0 && _initialVisibleColumnCount == 0)
            {
                _initialColumnCount = animator._columnCount;
                _initialVisibleColumnCount = animator._visibleColumnCount;
            }

            _lastSelectedLevelCategory = selectedLevelCategory;

            if (selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                animator._viewportTransform.localPosition = new Vector3(-animator._columnWidth / 2, 0, 0);
                // TODO: When adding enough columns to make it only one row, the scroll and animation are broken.
                __instance._gridView._columnCount += 10;
                __instance._gridView._visibleColumnCount -= 1;
            }
            else if (selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.MusicPacks)
            {
                animator._viewportTransform.localPosition = Vector3.zero;
                __instance._gridView._columnCount = _initialColumnCount;
                __instance._gridView._visibleColumnCount = _initialVisibleColumnCount;
            }

            Plugin.Log.Notice($"Column count is now {__instance._gridView._columnCount}");
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.GetContentXOffset))]
        private void ComputeNewXOffset(AnnotatedBeatmapLevelCollectionsGridViewAnimator __instance, ref float __result)
        {
            // TODO
        }
    }
}
