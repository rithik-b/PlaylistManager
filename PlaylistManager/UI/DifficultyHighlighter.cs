using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    public class DifficultyHighlighter : IBeatmapLevelUpdater, IDisposable, IInitializable
    {
        private readonly BeatmapCharacteristicSegmentedControlController _beatmapCharacteristicSegmentedControlController;
        private readonly IconSegmentedControl _beatmapCharacteristicSegmentedControl;
        private readonly BeatmapDifficultySegmentedControlController _beatmapDifficultySegmentedControlController;
        private readonly SegmentedControl _beatmapDifficultySegmentedControl;

        private IPlaylistSong _selectedPlaylistSong;
        private Color32? _originalDifficultyColor = null;

        internal event Action<bool> selectedDifficultyChanged;

        public DifficultyHighlighter(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            var standardLevelDetailView = standardLevelDetailViewController._standardLevelDetailView;
            _beatmapCharacteristicSegmentedControlController = standardLevelDetailView._beatmapCharacteristicSegmentedControlController;
            _beatmapCharacteristicSegmentedControl = _beatmapCharacteristicSegmentedControlController._segmentedControl;
            _beatmapDifficultySegmentedControlController = standardLevelDetailView._beatmapDifficultySegmentedControlController;
            _beatmapDifficultySegmentedControl = _beatmapDifficultySegmentedControlController._difficultySegmentedControl;
        }

        public void Initialize()
        {
            _beatmapCharacteristicSegmentedControl.didSelectCellEvent += BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            _beatmapDifficultySegmentedControl.didSelectCellEvent += BeatmapDifficultySegmentedControl_didSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent += BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void Dispose()
        {
            _beatmapCharacteristicSegmentedControl.didSelectCellEvent -= BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            _beatmapDifficultySegmentedControl.didSelectCellEvent -= BeatmapDifficultySegmentedControl_didSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent -= BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void BeatmapLevelUpdated(BeatmapLevel beatmapLevel)
        {
            if (beatmapLevel is PlaylistLevel playlistLevel)
            {
                _selectedPlaylistSong = playlistLevel.playlistSong;
            }
            else
            {
                _selectedPlaylistSong = null;
            }
        }

        private void HighlightDifficultiesForSelectedCharacteristic()
        {
            var difficultyCells = _beatmapDifficultySegmentedControl.cells;

            // TODO: Ugly, see if there's a better way to reset the color.
            var highlightColor = new Color32(255, 255, 0, 255);
            foreach (var difficultyCell in difficultyCells)
            {
                var text = difficultyCell.GetComponentInChildren<CurvedTextMeshPro>();
                _originalDifficultyColor ??= text.faceColor;
                if (text.faceColor.Compare(highlightColor))
                {
                    text.faceColor = _originalDifficultyColor.Value;
                }
            }

            if (_selectedPlaylistSong != null && _selectedPlaylistSong.Difficulties != null && _selectedPlaylistSong.Difficulties.Count != 0)
            {
                var difficultiesToHighlight = _selectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.Equals(_beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName, StringComparison.OrdinalIgnoreCase));
                var availaibleDifficulties = _beatmapDifficultySegmentedControlController._difficulties;

                foreach (var difficulty in difficultiesToHighlight)
                {
                    if (availaibleDifficulties.Contains(difficulty.BeatmapDifficulty))
                    {
                        var cellToHighlight = difficultyCells[_beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(difficulty.BeatmapDifficulty)];
                        var textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                        textToHighlight.faceColor = highlightColor;
                    }
                }
            }
        }

        private void RaiseDifficultyChangedEvent() => selectedDifficultyChanged?.Invoke(IsSelectedDifficultyHighlighted);

        internal void ToggleSelectedDifficultyHighlight()
        {
            if (_selectedPlaylistSong != null)
            {
                var difficultyCells = _beatmapDifficultySegmentedControl.cells;
                if (IsSelectedDifficultyHighlighted)
                {
                    _selectedPlaylistSong.Difficulties.RemoveAll(d => d.BeatmapDifficulty == _beatmapDifficultySegmentedControlController.selectedDifficulty);
                    var cellToUnhighlight = difficultyCells[_beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(_beatmapDifficultySegmentedControlController.selectedDifficulty)];
                    var textToUnhighlight = cellToUnhighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToUnhighlight.faceColor = new UnityEngine.Color32(255, 255, 255, 255);
                }
                else
                {
                    if (_selectedPlaylistSong.Difficulties == null)
                    {
                        _selectedPlaylistSong.Difficulties = new List<Difficulty>();
                    }
                    var difficulty = new Difficulty();
                    difficulty.BeatmapDifficulty = _beatmapDifficultySegmentedControlController.selectedDifficulty;
                    difficulty.Characteristic = _beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName;
                    _selectedPlaylistSong.AddDifficulty(difficulty);

                    var cellToHighlight = difficultyCells[_beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(_beatmapDifficultySegmentedControlController.selectedDifficulty)];
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
                if (_selectedPlaylistSong != null && _selectedPlaylistSong.Difficulties != null && _selectedPlaylistSong.Difficulties.Count != 0)
                {
                    var difficulties = _selectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.Equals(_beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName, StringComparison.OrdinalIgnoreCase));
                    return difficulties.Select(d => d.BeatmapDifficulty).Contains(_beatmapDifficultySegmentedControlController.selectedDifficulty);
                }
                return false;
            }
        }
    }
}
