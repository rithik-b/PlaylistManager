using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SiraUtil.Affinity;
using SongCore.Utilities;
using UnityEngine;

namespace PlaylistManager.AffinityPatches
{
    public class AnnotatedBeatmapLevelCollectionsGridViewAnimatorPatches : IAffinity
    {
        private readonly AnnotatedBeatmapLevelCollectionsViewController _annotatedBeatmapLevelCollectionsTableViewController;
        private readonly SelectLevelCategoryViewController _selectLevelCategoryViewController;

        private int _originalColumnCount;
        private bool _isGridViewResized;

        public AnnotatedBeatmapLevelCollectionsGridViewAnimatorPatches(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController, SelectLevelCategoryViewController selectLevelCategoryViewController)
        {
            _annotatedBeatmapLevelCollectionsTableViewController = annotatedBeatmapLevelCollectionsTableViewController;
            _selectLevelCategoryViewController = selectLevelCategoryViewController;
        }

        // TODO: Doesn't resize on internal restart.
        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.SetData))]
        [AffinityPrefix]
        private void AddColumnsAndResize(AnnotatedBeatmapLevelCollectionsGridView __instance, IReadOnlyList<BeatmapLevelPack> annotatedBeatmapLevelCollections)
        {
            if (__instance._gridView._dataSource == null)
            {
                return;
            }

            if (_originalColumnCount == default)
            {
                _originalColumnCount = __instance._gridView._columnCount;
            }

            var selectedLevelCategory = _selectLevelCategoryViewController.selectedLevelCategory;
            var animator = _annotatedBeatmapLevelCollectionsTableViewController._annotatedBeatmapLevelCollectionsGridView._animator;

            if (selectedLevelCategory == SelectLevelCategoryViewController.LevelCategory.CustomSongs)
            {
                // Number of columns for max visible row count before it starts clipping with the ground.
                __instance._gridView._columnCount = Mathf.CeilToInt((annotatedBeatmapLevelCollections?.Count ?? 0) / 5f);

                if (!_isGridViewResized)
                {
                    __instance._gridView._visibleColumnCount -= 1;

                    var rectTransform = (RectTransform)__instance._gridView.transform;
                    rectTransform.sizeDelta -= new Vector2(animator._columnWidth, 0);
                    rectTransform.anchoredPosition -= new Vector2(animator._columnWidth / 2, 0);

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
                    rectTransform.sizeDelta += new Vector2(animator._columnWidth, 0);
                    rectTransform.anchoredPosition += new Vector2(animator._columnWidth / 2, 0);

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
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> FixViewportWidth(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch(i => i.opcode == OpCodes.Call && i.operand as ConstructorInfo == AccessTools.Constructor(typeof(Vector2), new[] { typeof(float), typeof(float) })))
                .ThrowIfInvalid()
                .Advance(-2)
                .RemoveInstruction()
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    Transpilers.EmitDelegate<Func<AnnotatedBeatmapLevelCollectionsGridViewAnimator, float>>(animator =>
                        ((animator._columnCount - animator._visibleColumnCount) * 2 + animator._visibleColumnCount) * animator._columnWidth))
                .InstructionEnumeration();
        }
    }
}
