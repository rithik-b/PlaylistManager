using HMUI;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine.UI;

namespace PlaylistManager.Utilities
{
    internal class Accessors
    {
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, Image>.Accessor CoverImageAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, Image>.GetAccessor("_coverImage");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, IAnnotatedBeatmapLevelCollection>.Accessor BeatmapCollectionAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, IAnnotatedBeatmapLevelCollection>.GetAccessor("_annotatedBeatmapLevelCollection");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.Accessor AnnotatedBeatmapLevelCollectionsTableViewAccessor = FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.GetAccessor("_annotatedBeatmapLevelCollectionsTableView");
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
