using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SiraUtil.Affinity;
using SongCore.Utilities;
using UnityEngine;

namespace PlaylistManager.AffinityPatches
{
    public class AnnotatedBeatmapLevelCollectionsGridViewAnimatorPatches : IAffinity
    {
        private readonly SelectLevelCategoryViewController _selectLevelCategoryViewController;

        private int _originalColumnCount;
        private Vector2 _originalScreenSize;
        private bool _isGridViewResized;

        public AnnotatedBeatmapLevelCollectionsGridViewAnimatorPatches(SelectLevelCategoryViewController selectLevelCategoryViewController)
        {
            _selectLevelCategoryViewController = selectLevelCategoryViewController;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.SetData))]
        [AffinityPrefix]
        private void ResizeGridView(AnnotatedBeatmapLevelCollectionsGridView __instance, IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections)
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

                if (!_isGridViewResized)
                {
                    __instance._gridView._visibleColumnCount -= 1;

                    var rectTransform = (RectTransform)__instance._gridView.transform;
                    rectTransform.sizeDelta -= new Vector2(__instance._cellWidth, 0);
                    rectTransform.anchoredPosition -= new Vector2(__instance._cellWidth / 2, 0);

                    _isGridViewResized = true;
                }
            }
            else if (selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.MusicPacks)
            {
                __instance._gridView._columnCount = _originalColumnCount;

                if (_isGridViewResized)
                {
                    __instance._gridView._visibleColumnCount += 1;

                    var rectTransform = (RectTransform)__instance._gridView.transform;
                    rectTransform.sizeDelta += new Vector2(__instance._cellWidth, 0);
                    rectTransform.anchoredPosition += new Vector2(__instance._cellWidth / 2, 0);

                    _isGridViewResized = false;
                }
            }
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.GetContentXOffset))]
        private void ComputeNewXOffset(AnnotatedBeatmapLevelCollectionsGridViewAnimator __instance, ref float __result)
        {
            var zeroOffset = (__instance._columnCount - 1) / 2f;
            var maxMove = (__instance._columnCount - __instance._visibleColumnCount) / 2f;
            var toMove = zeroOffset - __instance._selectedColumn;
            if (__instance._visibleColumnCount % 2 == 0)
            {
                toMove -= 0.5f;
            }

            __result = Math.Clamp(toMove, -maxMove, maxMove) * __instance._columnWidth;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.OnPointerEnter))]
        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.OnPointerExit))]
        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.HandleCellSelectionDidChange))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> OpenCollectionWithOneRow(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_1))
                .ThrowIfInvalid()
                .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0)
                .InstructionEnumeration();
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridViewAnimator), nameof(AnnotatedBeatmapLevelCollectionsGridViewAnimator.AnimateOpen))]
        private void ChangeGridAndScreenSize(AnnotatedBeatmapLevelCollectionsGridViewAnimator __instance, bool animated)
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

            if (_isGridViewResized)
            {
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

                rectTransform.sizeDelta = new Vector2(_originalScreenSize.x + (__instance._columnCount - __instance._visibleColumnCount - 1) * __instance._columnWidth * 2, _originalScreenSize.y);
            }
        }
    }
}
