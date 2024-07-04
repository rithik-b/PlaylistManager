using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SongCore.Utilities;
using UnityEngine;

namespace PlaylistManager.AffinityPatches
{
    internal class AnnotatedBeatmapLevelCollectionsUIPatches : IAffinity
    {
        private readonly SelectLevelCategoryViewController _selectLevelCategoryViewController;
        private readonly SiraLog _logger;

        private int _originalColumnCount;
        private Vector2 _originalScreenSize;
        private bool _isGridResized;

        private AnnotatedBeatmapLevelCollectionsUIPatches(SelectLevelCategoryViewController selectLevelCategoryViewController, SiraLog logger)
        {
            _selectLevelCategoryViewController = selectLevelCategoryViewController;
            _logger = logger;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.SetData))]
        [AffinityPrefix]
        private void ResizeGrid(AnnotatedBeatmapLevelCollectionsGridView __instance, IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections)
        {
            if (_originalColumnCount == default)
            {
                _originalColumnCount = __instance._gridView._columnCount;
            }

            var selectedLevelCategory = _selectLevelCategoryViewController.selectedLevelCategory;
            if (selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                // Number of columns for max visible row count before it starts clipping with the ground.
                __instance._gridView._columnCount = Mathf.CeilToInt((annotatedBeatmapLevelCollections?.Count ?? 0) / 5f);

                // Remove one column to make room for our buttons.
                if (!_isGridResized)
                {
                    __instance._gridView._visibleColumnCount -= 1;

                    var rectTransform = (RectTransform)__instance._gridView.transform;
                    rectTransform.sizeDelta -= new Vector2(__instance._cellWidth, 0);
                    rectTransform.anchoredPosition -= new Vector2(__instance._cellWidth / 2, 0);

                    _isGridResized = true;
                }
            }
            else if (selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.MusicPacks)
            {
                __instance._gridView._columnCount = _originalColumnCount;

                // Restore the removed column since we don't want to show an empty cell.
                if (_isGridResized)
                {
                    __instance._gridView._visibleColumnCount += 1;

                    var rectTransform = (RectTransform)__instance._gridView.transform;
                    rectTransform.sizeDelta += new Vector2(__instance._cellWidth, 0);
                    rectTransform.anchoredPosition += new Vector2(__instance._cellWidth / 2, 0);

                    _isGridResized = false;
                }
            }
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.OnPointerEnter))]
        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.OnPointerExit))]
        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.HandleCellSelectionDidChange))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> MakeGridInteractableWithOneRow(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_1))
                .ThrowIfInvalid()
                .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0)
                .InstructionEnumeration();
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.GetContentXOffset))]
        private void RecalculateContentXOffsetBasedOnColumnCount(AnnotatedBeatmapLevelCollectionsGridViewAnimator __instance, ref float __result)
        {
            var zeroOffset = (__instance._columnCount - 1) / 2f;
            var maxMove = (__instance._columnCount - __instance._visibleColumnCount) / 2f;
            var toMove = zeroOffset - __instance._selectedColumn;
            if (__instance._visibleColumnCount % 2 == 0)
            {
                toMove -= 0.5f;
            }

            // TODO: Remove.
            _logger.Debug($"_columnCount: {__instance._columnCount}, _visibleColumnCount: {__instance._visibleColumnCount}");

            __result = Math.Clamp(toMove, -maxMove, maxMove) * __instance._columnWidth;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.AnimateOpen))]
        private void RecalculateSizeBasedOnColumnCount(AnnotatedBeatmapLevelCollectionsGridViewAnimator __instance, bool animated)
        {
            var x = ((__instance._columnCount - __instance._visibleColumnCount) * 2 + __instance._visibleColumnCount) * __instance._columnWidth;
            if (animated)
            {
                __instance._viewportSizeTween.toValue = new Vector2(x, __instance._viewportSizeTween.toValue.y);
            }
            else
            {
                __instance._viewportTransform.sizeDelta = new Vector2(x, __instance._viewportTransform.sizeDelta.y);
            }

            if (_isGridResized)
            {
                // It would otherwise fly away when setting the Screen size.
                var rectTransform = (RectTransform)_selectLevelCategoryViewController.transform;

                if (rectTransform.anchorMin.x == 0 || rectTransform.anchorMax.x == 0)
                {
                    var localPosition = rectTransform.localPosition;
                    rectTransform.anchorMin = new Vector2(0.5f, rectTransform.anchorMin.y);
                    rectTransform.anchorMax = new Vector2(0.5f, rectTransform.anchorMax.y);
                    rectTransform.localPosition = localPosition;
                }

                rectTransform = (RectTransform)__instance.gameObject.GetComponentInParent<HMUI.Screen>().transform;

                if (_originalScreenSize == default)
                {
                    _originalScreenSize = rectTransform.sizeDelta;
                }

                // Resizing Screen is needed to allow the hover hint to be shown when the GridView is larger.
                rectTransform.sizeDelta = new Vector2(_originalScreenSize.x + (__instance._columnCount - __instance._visibleColumnCount - 1) * __instance._columnWidth * 2, _originalScreenSize.y);
            }
        }
    }
}
