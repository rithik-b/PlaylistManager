using HMUI;
using IPA.Utilities;
using System.Collections.Generic;

namespace PlaylistManager.Utilities
{
    internal class Accessors
    {
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionCell, ImageView>.Accessor CoverImageAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionCell, ImageView>.GetAccessor("_coverImage");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionCell, IAnnotatedBeatmapLevelCollection>.Accessor BeatmapCollectionAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionCell, IAnnotatedBeatmapLevelCollection>.GetAccessor("_annotatedBeatmapLevelCollection");

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsGridView>.Accessor AnnotatedBeatmapLevelCollectionsGridViewAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsGridView>.GetAccessor("_annotatedBeatmapLevelCollectionsGridView");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsGridView, AnnotatedBeatmapLevelCollectionsGridViewAnimator>.Accessor AnnotatedBeatmapLevelCollectionsGridViewAnimatorAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionsGridView, AnnotatedBeatmapLevelCollectionsGridViewAnimator>.GetAccessor("_animator");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, IReadOnlyList<IAnnotatedBeatmapLevelCollection>>.Accessor AnnotatedBeatmapLevelCollectionsAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, IReadOnlyList<IAnnotatedBeatmapLevelCollection>>.GetAccessor("_annotatedBeatmapLevelCollections");
        public static readonly FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.Accessor CustomLevelPackCollectionAccessor = FieldAccessor<BeatmapLevelsModel, IBeatmapLevelPackCollection>.GetAccessor("_customLevelPackCollection");
        public static readonly FieldAccessor<LevelCollectionViewController, LevelCollectionTableView>.Accessor LevelCollectionTableViewAccessor = FieldAccessor<LevelCollectionViewController, LevelCollectionTableView>.GetAccessor("_levelCollectionTableView");
        public static readonly FieldAccessor<LevelCollectionTableView, HashSet<string>>.Accessor FavoriteLevelIdsAccessor = FieldAccessor<LevelCollectionTableView, HashSet<string>>.GetAccessor("_favoriteLevelIds");
        public static readonly FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.Accessor StandardLevelDetailViewAccessor = FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.GetAccessor("_standardLevelDetailView");
        public static readonly FieldAccessor<StandardLevelDetailView, BeatmapCharacteristicSegmentedControlController>.Accessor BeatmapCharacteristicSegmentedControlController = FieldAccessor<StandardLevelDetailView, BeatmapCharacteristicSegmentedControlController>.GetAccessor("_beatmapCharacteristicSegmentedControlController");
        public static readonly FieldAccessor<BeatmapCharacteristicSegmentedControlController, IconSegmentedControl>.Accessor BeatmapCharacteristicsSegmentedControlAccessor = FieldAccessor<BeatmapCharacteristicSegmentedControlController, IconSegmentedControl>.GetAccessor("_segmentedControl");
        public static readonly FieldAccessor<StandardLevelDetailView, BeatmapDifficultySegmentedControlController>.Accessor BeatmapDifficultySegmentedControlControllerAccessor = FieldAccessor<StandardLevelDetailView, BeatmapDifficultySegmentedControlController>.GetAccessor("_beatmapDifficultySegmentedControlController");
        public static readonly FieldAccessor<BeatmapDifficultySegmentedControlController, TextSegmentedControl>.Accessor BeatmapDifficultySegmentedControlAccessor = FieldAccessor<BeatmapDifficultySegmentedControlController, TextSegmentedControl>.GetAccessor("_difficultySegmentedControl");
        public static readonly FieldAccessor<BeatmapDifficultySegmentedControlController, List<BeatmapDifficulty>>.Accessor DifficultiesAccessor = FieldAccessor<BeatmapDifficultySegmentedControlController, List<BeatmapDifficulty>>.GetAccessor("_difficulties");
        public static readonly FieldAccessor<SegmentedControl, List<SegmentedControlCell>>.Accessor SegmentedControllerCellsAccessor = FieldAccessor<SegmentedControl, List<SegmentedControlCell>>.GetAccessor("_cells");
    }
}
