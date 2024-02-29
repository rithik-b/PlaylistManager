using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace PlaylistManager.UI
{
    public class DifficultyHighlighter : IPreviewBeatmapLevelUpdater, IDisposable, IInitializable
    {
        private BeatmapCharacteristicSegmentedControlController beatmapCharacteristicSegmentedControlController;
        private readonly IconSegmentedControl beatmapCharacteristicSegmentedControl;
        private BeatmapDifficultySegmentedControlController beatmapDifficultySegmentedControlController;
        private SegmentedControl beatmapDifficultySegmentedControl;
        private IPlaylistSong selectedPlaylistSong;

        internal event Action<bool> selectedDifficultyChanged;

        public DifficultyHighlighter(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            var standardLevelDetailView = standardLevelDetailViewController._standardLevelDetailView;
            beatmapCharacteristicSegmentedControlController = standardLevelDetailView._beatmapCharacteristicSegmentedControlController;
            beatmapCharacteristicSegmentedControl = beatmapCharacteristicSegmentedControlController._segmentedControl;
            beatmapDifficultySegmentedControlController = standardLevelDetailView._beatmapDifficultySegmentedControlController;
            beatmapDifficultySegmentedControl = beatmapDifficultySegmentedControlController._difficultySegmentedControl;
        }

        public void Initialize()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent += BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            beatmapDifficultySegmentedControl.didSelectCellEvent += BeatmapDifficultySegmentedControl_didSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent += BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void Dispose()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent -= BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            beatmapDifficultySegmentedControl.didSelectCellEvent -= BeatmapDifficultySegmentedControl_didSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent -= BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void PreviewBeatmapLevelUpdated(BeatmapLevel beatmapLevel)
        {
            if (beatmapLevel is PlaylistLevel selectedPlaylistSong)
            {
                this.selectedPlaylistSong = selectedPlaylistSong.playlistSong;
            }
            else
            {
                this.selectedPlaylistSong = null;
            }
        }

        private void HighlightDifficultiesForSelectedCharacteristic()
        {
            if (selectedPlaylistSong != null && selectedPlaylistSong.Difficulties != null && selectedPlaylistSong.Difficulties.Count != 0)
            {
                var difficultiesToHighlight = selectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.Equals(beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName, StringComparison.OrdinalIgnoreCase));
                var availaibleDifficulties = beatmapDifficultySegmentedControlController._difficulties;
                var difficultyCells = beatmapDifficultySegmentedControl.cells;

                foreach (var difficulty in difficultiesToHighlight)
                {
                    if (availaibleDifficulties.Contains(difficulty.BeatmapDifficulty))
                    {
                        var cellToHighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(difficulty.BeatmapDifficulty)];
                        var textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                        textToHighlight.faceColor = new UnityEngine.Color32(255, 255, 0, 255);
                    }
                }
            }
        }

        private void RaiseDifficultyChangedEvent() => selectedDifficultyChanged?.Invoke(IsSelectedDifficultyHighlighted);

        internal void ToggleSelectedDifficultyHighlight()
        {
            if (selectedPlaylistSong != null)
            {
                var difficultyCells = beatmapDifficultySegmentedControl.cells;
                if (IsSelectedDifficultyHighlighted)
                {
                    selectedPlaylistSong.Difficulties.RemoveAll(d => d.BeatmapDifficulty == beatmapDifficultySegmentedControlController.selectedDifficulty);
                    var cellToUnhighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(beatmapDifficultySegmentedControlController.selectedDifficulty)];
                    var textToUnhighlight = cellToUnhighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToUnhighlight.faceColor = new UnityEngine.Color32(255, 255, 255, 255);
                }
                else
                {
                    if (selectedPlaylistSong.Difficulties == null)
                    {
                        selectedPlaylistSong.Difficulties = new List<Difficulty>();
                    }
                    var difficulty = new Difficulty();
                    difficulty.BeatmapDifficulty = beatmapDifficultySegmentedControlController.selectedDifficulty;
                    difficulty.Characteristic = beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName;
                    selectedPlaylistSong.AddDifficulty(difficulty);

                    var cellToHighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(beatmapDifficultySegmentedControlController.selectedDifficulty)];
                    var textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToHighlight.faceColor = new UnityEngine.Color32(255, 255, 0, 255);
                }
            }

        }

        private void BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent()
        {
            HighlightDifficultiesForSelectedCharacteristic();
            RaiseDifficultyChangedEvent();
        }

        private void BeatmapCharacteristicSegmentedControl_DidSelectCellEvent(SegmentedControl _, int __)
        {
            HighlightDifficultiesForSelectedCharacteristic();
        }

        private void BeatmapDifficultySegmentedControl_didSelectCellEvent(SegmentedControl arg1, int arg2)
        {
            RaiseDifficultyChangedEvent();
        }

        private bool IsSelectedDifficultyHighlighted
        {
            get
            {
                if (selectedPlaylistSong != null && selectedPlaylistSong.Difficulties != null && selectedPlaylistSong.Difficulties.Count != 0)
                {
                    var difficulties = selectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.Equals(beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName, StringComparison.OrdinalIgnoreCase));
                    return difficulties.Select(d => d.BeatmapDifficulty).Contains(beatmapDifficultySegmentedControlController.selectedDifficulty);
                }
                return false;
            }
        }
    }
}
