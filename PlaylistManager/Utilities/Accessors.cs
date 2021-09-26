using HMUI;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistManager.Utilities
{
    internal class Accessors
    {
        #region AnnotatedBeatmapLevelCollectionCell

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionCell, ImageView>.Accessor CoverImageAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionCell, ImageView>.GetAccessor("_coverImage");

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionCell, IAnnotatedBeatmapLevelCollection>.Accessor BeatmapCollectionAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionCell, IAnnotatedBeatmapLevelCollection>.GetAccessor("_annotatedBeatmapLevelCollection");

        #endregion

        #region ScrollView

        public static readonly FieldAccessor<ScrollView, Button>.Accessor PageUpAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageUpButton");

        public static readonly FieldAccessor<ScrollView, Button>.Accessor PageDownAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageDownButton");

        public static readonly FieldAccessor<ScrollView, VerticalScrollIndicator>.Accessor ScrollIndicatorAccessor = 
            FieldAccessor<ScrollView, VerticalScrollIndicator>.GetAccessor("_verticalScrollIndicator");

        public static readonly FieldAccessor<ScrollView, IVRPlatformHelper>.Accessor PlatformHelperAccessor =
            FieldAccessor<ScrollView, IVRPlatformHelper>.GetAccessor("_platformHelper");

        #endregion

        #region AnnotatedBeatmapLevelCollectionsViewController

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsGridView>.Accessor AnnotatedBeatmapLevelCollectionsGridViewAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsGridView>.GetAccessor("_annotatedBeatmapLevelCollectionsGridView");

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, IReadOnlyList<IAnnotatedBeatmapLevelCollection>>.Accessor AnnotatedBeatmapLevelCollectionsAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, IReadOnlyList<IAnnotatedBeatmapLevelCollection>>.GetAccessor("_annotatedBeatmapLevelCollections");

        #endregion

        #region AnnotatedBeatmapLevelCollectionsGridView

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsGridView, AnnotatedBeatmapLevelCollectionsGridViewAnimator>.Accessor GridViewAnimatorAccessor =
            FieldAccessor<AnnotatedBeatmapLevelCollectionsGridView, AnnotatedBeatmapLevelCollectionsGridViewAnimator>.GetAccessor("_animator");

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsGridView, PageControl>.Accessor PageControlAccessor =
            FieldAccessor<AnnotatedBeatmapLevelCollectionsGridView, PageControl>.GetAccessor("_pageControl");

        #endregion

        #region AnnotatedBeatmapLevelCollectionsGridViewAnimator

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsGridViewAnimator, RectTransform>.Accessor ViewportAccessor = 
            FieldAccessor<AnnotatedBeatmapLevelCollectionsGridViewAnimator, RectTransform>.GetAccessor("_viewportTransform");

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsGridViewAnimator, RectTransform>.Accessor ContentAccessor =
            FieldAccessor<AnnotatedBeatmapLevelCollectionsGridViewAnimator, RectTransform>.GetAccessor("_contentTransform");

        #endregion

        #region StandardLevelDetailView

        public static readonly FieldAccessor<StandardLevelDetailView, BeatmapCharacteristicSegmentedControlController>.Accessor BeatmapCharacteristicSegmentedControlController = 
            FieldAccessor<StandardLevelDetailView, BeatmapCharacteristicSegmentedControlController>.GetAccessor("_beatmapCharacteristicSegmentedControlController");

        public static readonly FieldAccessor<StandardLevelDetailView, BeatmapDifficultySegmentedControlController>.Accessor BeatmapDifficultySegmentedControlControllerAccessor =
            FieldAccessor<StandardLevelDetailView, BeatmapDifficultySegmentedControlController>.GetAccessor("_beatmapDifficultySegmentedControlController");

        #endregion

        #region BeatmapDifficultySegmentedControlController

        public static readonly FieldAccessor<BeatmapDifficultySegmentedControlController, TextSegmentedControl>.Accessor BeatmapDifficultySegmentedControlAccessor =
            FieldAccessor<BeatmapDifficultySegmentedControlController, TextSegmentedControl>.GetAccessor("_difficultySegmentedControl");

        public static readonly FieldAccessor<BeatmapDifficultySegmentedControlController, List<BeatmapDifficulty>>.Accessor DifficultiesAccessor =
            FieldAccessor<BeatmapDifficultySegmentedControlController, List<BeatmapDifficulty>>.GetAccessor("_difficulties");

        #endregion

        #region Other

        public static readonly FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.Accessor CustomLevelPackCollectionAccessor =
            FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.GetAccessor("_customLevelPackCollection");

        public static readonly FieldAccessor<LevelCollectionViewController, LevelCollectionTableView>.Accessor LevelCollectionTableViewAccessor =
            FieldAccessor<LevelCollectionViewController, LevelCollectionTableView>.GetAccessor("_levelCollectionTableView");

        public static readonly FieldAccessor<LevelCollectionTableView, HashSet<string>>.Accessor FavoriteLevelIdsAccessor =
            FieldAccessor<LevelCollectionTableView, HashSet<string>>.GetAccessor("_favoriteLevelIds");

        public static readonly FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.Accessor StandardLevelDetailViewAccessor =
            FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.GetAccessor("_standardLevelDetailView");

        public static readonly FieldAccessor<BeatmapCharacteristicSegmentedControlController, IconSegmentedControl>.Accessor BeatmapCharacteristicsSegmentedControlAccessor =
            FieldAccessor<BeatmapCharacteristicSegmentedControlController, IconSegmentedControl>.GetAccessor("_segmentedControl");

        public static readonly FieldAccessor<SegmentedControl, List<SegmentedControlCell>>.Accessor SegmentedControllerCellsAccessor = 
            FieldAccessor<SegmentedControl, List<SegmentedControlCell>>.GetAccessor("_cells");
        
        #endregion
    }
}
