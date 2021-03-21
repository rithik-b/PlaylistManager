using BeatSaberPlaylistsLib.Types;
using HMUI;
using IPA.Utilities;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using System;
using System.Collections.Generic;
using Zenject;

namespace PlaylistManager.UI
{
    class DifficultyHighlighter : IPreviewBeatmapLevelUpdater, IDisposable, IInitializable
    {
        private BeatmapCharacteristicSegmentedControlController beatmapCharacteristicSegmentedControlController;
        private readonly IconSegmentedControl beatmapCharacteristicSegmentedControl;
        private BeatmapDifficultySegmentedControlController beatmapDifficultySegmentedControlController;
        private SegmentedControl beatmapDifficultySegmentedControl;

        private PlaylistSong _selectedPlaylistSong;

        public static readonly FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.Accessor StandardLevelDetailViewAccessor = FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.GetAccessor("_standardLevelDetailView");
        public static readonly FieldAccessor<StandardLevelDetailView, BeatmapCharacteristicSegmentedControlController>.Accessor BeatmapCharacteristicSegmentedControlController = FieldAccessor<StandardLevelDetailView, BeatmapCharacteristicSegmentedControlController>.GetAccessor("_beatmapCharacteristicSegmentedControlController");
        public static readonly FieldAccessor<BeatmapCharacteristicSegmentedControlController, IconSegmentedControl>.Accessor BeatmapCharacteristicsSegmentedControlAccessor = FieldAccessor<BeatmapCharacteristicSegmentedControlController, IconSegmentedControl>.GetAccessor("_segmentedControl");
        public static readonly FieldAccessor<StandardLevelDetailView, BeatmapDifficultySegmentedControlController>.Accessor BeatmapDifficultySegmentedControlControllerAccessor = FieldAccessor<StandardLevelDetailView, BeatmapDifficultySegmentedControlController>.GetAccessor("_beatmapDifficultySegmentedControlController");
        public static readonly FieldAccessor<BeatmapDifficultySegmentedControlController, TextSegmentedControl>.Accessor BeatmapDifficultySegmentedControlAccessor = FieldAccessor<BeatmapDifficultySegmentedControlController, TextSegmentedControl>.GetAccessor("_difficultySegmentedControl");
        public static readonly FieldAccessor<SegmentedControl, List<SegmentedControlCell>>.Accessor SegmentedControllerCellsAccessor = FieldAccessor<SegmentedControl, List<SegmentedControlCell>>.GetAccessor("_cells");


        public DifficultyHighlighter(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            StandardLevelDetailView standardLevelDetailView = StandardLevelDetailViewAccessor(ref standardLevelDetailViewController);
            beatmapCharacteristicSegmentedControlController = BeatmapCharacteristicSegmentedControlController(ref standardLevelDetailView);
            beatmapCharacteristicSegmentedControl = BeatmapCharacteristicsSegmentedControlAccessor(ref beatmapCharacteristicSegmentedControlController);
            beatmapDifficultySegmentedControlController = BeatmapDifficultySegmentedControlControllerAccessor(ref standardLevelDetailView);
            beatmapDifficultySegmentedControl = BeatmapDifficultySegmentedControlAccessor(ref beatmapDifficultySegmentedControlController);
        }

        public void Initialize()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent += HighlightDifficultiesForSelectedCharacteristic;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent += BeatmapCharacteristicSegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void Dispose()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent -= HighlightDifficultiesForSelectedCharacteristic;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent -= BeatmapCharacteristicSegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if (beatmapLevel is PlaylistSong selectedPlaylistSong && selectedPlaylistSong.Difficulties != null && selectedPlaylistSong.Difficulties.Count != 0)
            {
                SelectedPlaylistSong = selectedPlaylistSong;
            }
            else
            {
                SelectedPlaylistSong = null;
                beatmapCharacteristicSegmentedControl.didSelectCellEvent -= HighlightDifficultiesForSelectedCharacteristic;
            }
        }

        private void BeatmapCharacteristicSegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent()
        {
            HighlightDifficultiesForSelectedCharacteristic(null, 0);
        }

        private void HighlightDifficultiesForSelectedCharacteristic(SegmentedControl _, int __)
        {
            if (SelectedPlaylistSong != null)
            {
                List<Difficulty> difficulties = SelectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.ToUpper() == beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName.ToUpper());
                List<SegmentedControlCell> difficultyCells = SegmentedControllerCellsAccessor(ref beatmapDifficultySegmentedControl);

                foreach (var difficulty in difficulties)
                {
                    SegmentedControlCell cellToHighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(difficulty.BeatmapDifficulty)];
                    CurvedTextMeshPro textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToHighlight.faceColor = new UnityEngine.Color32(255, 255, 0, 255);
                }
            }
        }

        private PlaylistSong SelectedPlaylistSong
        {
            get => _selectedPlaylistSong;
            set
            {
                _selectedPlaylistSong = value;
            }
        }
    }
}
