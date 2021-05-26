using BeatSaberPlaylistsLib.Types;
using HMUI;
using IPA.Utilities;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
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

        public DifficultyHighlighter(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            StandardLevelDetailView standardLevelDetailView = Accessors.StandardLevelDetailViewAccessor(ref standardLevelDetailViewController);
            beatmapCharacteristicSegmentedControlController = Accessors.BeatmapCharacteristicSegmentedControlController(ref standardLevelDetailView);
            beatmapCharacteristicSegmentedControl = Accessors.BeatmapCharacteristicsSegmentedControlAccessor(ref beatmapCharacteristicSegmentedControlController);
            beatmapDifficultySegmentedControlController = Accessors.BeatmapDifficultySegmentedControlControllerAccessor(ref standardLevelDetailView);
            beatmapDifficultySegmentedControl = Accessors.BeatmapDifficultySegmentedControlAccessor(ref beatmapDifficultySegmentedControlController);
        }

        public void Initialize()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent += BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent += HighlightDifficultiesForSelectedCharacteristic;
        }

        public void Dispose()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent -= BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent -= HighlightDifficultiesForSelectedCharacteristic;
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
            }
        }

        private void HighlightDifficultiesForSelectedCharacteristic()
        {
            if (SelectedPlaylistSong != null)
            {
                List<Difficulty> difficulties = SelectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.ToUpper() == beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName.ToUpper());
                List<SegmentedControlCell> difficultyCells = Accessors.SegmentedControllerCellsAccessor(ref beatmapDifficultySegmentedControl);

                foreach (var difficulty in difficulties)
                {
                    SegmentedControlCell cellToHighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(difficulty.BeatmapDifficulty)];
                    CurvedTextMeshPro textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToHighlight.faceColor = new UnityEngine.Color32(255, 255, 0, 255);
                }
            }
        }
        private void BeatmapCharacteristicSegmentedControl_DidSelectCellEvent(SegmentedControl _, int __)
        {
            HighlightDifficultiesForSelectedCharacteristic();
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
