using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using UnityEngine;
using Zenject;

namespace PlaylistManager.UI
{
    // TODO: calling RaisePlaylistChanged everywhere kind of sucks. is there a better way to do this?
    internal class PlaylistUpdater : IInitializable, IDisposable, ILevelCollectionUpdater
    {
        private readonly HashSet<IPlaylist> playlistReferences = new();
        private readonly AnnotatedBeatmapLevelCollectionsViewController _annotatedBeatmapLevelCollectionsViewController;
        private readonly LevelCollectionNavigationController _levelCollectionNavigationController;
        private readonly LevelPackDetailViewController _levelPackDetailViewController;

        private IPlaylist _selectedPlaylist;

        private PlaylistUpdater(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController, LevelCollectionNavigationController levelCollectionNavigationController, LevelPackDetailViewController levelPackDetailViewController)
        {
            _annotatedBeatmapLevelCollectionsViewController = annotatedBeatmapLevelCollectionsViewController;
            _levelCollectionNavigationController = levelCollectionNavigationController;
            _levelPackDetailViewController = levelPackDetailViewController;
        }

        public void Initialize()
        {
            foreach (var playlist in PlaylistLibUtils.TryGetAllPlaylists())
            {
                playlist.PlaylistChanged += UpdatePlaylist;
            }

            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested += HandleDidRequestPlaylistsRefresh;
        }

        public void Dispose()
        {
            foreach (var playlist in playlistReferences)
            {
                playlist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }

            foreach (var playlist in PlaylistLibUtils.TryGetAllPlaylists())
            {
                playlist.PlaylistChanged -= UpdatePlaylist;
            }

            PlaylistLibUtils.playlistManager.PlaylistsRefreshRequested -= HandleDidRequestPlaylistsRefresh;
        }

        private void HandleDidRequestPlaylistsRefresh(object sender, string e)
        {
            RefreshPlaylistChangedListeners();
        }

        public void RefreshPlaylistChangedListeners(BeatmapLevelPack[] beatmapLevelPacks = null)
        {
            foreach (var playlist in beatmapLevelPacks?.Select(p => ((PlaylistLevelPack)p).playlist) ?? PlaylistLibUtils.TryGetAllPlaylists())
            {
                playlist.PlaylistChanged -= UpdatePlaylist;
                playlist.PlaylistChanged += UpdatePlaylist;
            }
        }

        public void UpdatePlaylist(object sender, EventArgs e)
        {
            UpdatePlaylist((IPlaylist)sender);
        }

        private void UpdatePlaylist(IPlaylist playlist)
        {
            var playlistLevelPack = RefreshAnnotatedBeatmapCollection(playlist.PlaylistLevelPack);

            if (playlistLevelPack == null)
            {
                return;
            }

            _levelPackDetailViewController._pack = playlistLevelPack;
            _levelPackDetailViewController.ShowContent(LevelPackDetailViewController.ContentType.NonBuyable);

            _levelCollectionNavigationController._levelPack = playlistLevelPack;

            var levelCollectionTableView = _levelCollectionNavigationController._levelCollectionViewController._levelCollectionTableView;
            levelCollectionTableView._headerText = playlistLevelPack.packName;
            levelCollectionTableView._tableView.RefreshCellsContent();
        }

        public BeatmapLevelPack RefreshAnnotatedBeatmapCollection(BeatmapLevelPack beatmapLevelPack)
        {
            var annotatedBeatmapLevelCollections = _annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollections.ToArray();
            var i = Array.FindIndex(annotatedBeatmapLevelCollections, pack => pack is PlaylistLevelPack && pack.packID == beatmapLevelPack.packID);

            if (i == -1)
            {
                return null;
            }

            var playlistLevelPack = ((PlaylistLevelPack)beatmapLevelPack).playlist.PlaylistLevelPack;

            annotatedBeatmapLevelCollections[i] = playlistLevelPack;
            _annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollections = annotatedBeatmapLevelCollections;
            _annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._annotatedBeatmapLevelCollections = annotatedBeatmapLevelCollections;

            var gridView = _annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._gridView;
            var cellWidth = gridView._dataSource.cellWidth;
            var cellHeight = gridView._dataSource.cellHeight;
            foreach (var prefab in gridView._spawnedCellsPerPrefabDictionary.Keys)
            {
                var spawnedCells = gridView._spawnedCellsPerPrefabDictionary[prefab];
                foreach (var spawnedCell in spawnedCells)
                {
                    if (((AnnotatedBeatmapLevelCollectionCell)spawnedCell)._beatmapLevelPack.packID == playlistLevelPack.packID)
                    {
                        spawnedCell.gameObject.SetActive(false);
                        gridView._availableCellsPerPrefabDictionary[prefab].Enqueue(spawnedCell);
                        spawnedCells.Remove(spawnedCell);

                        break;
                    }
                }
            }

            var columnIndex = i % gridView._columnCount;
            var rowIndex = i / gridView._columnCount;
            var transform = (RectTransform)gridView._dataSource.CellForIdx(gridView, i).transform;
            transform.anchorMin = new Vector2(0.0f, 1f);
            transform.anchorMax = new Vector2(0.0f, 1f);
            transform.pivot = new Vector2(0.0f, 1f);
            transform.anchoredPosition = new Vector2(columnIndex * cellWidth, rowIndex * -cellHeight);

            return playlistLevelPack;
        }

        private void SelectedPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (_levelPackDetailViewController._blurredPackArtwork != null)
            {
                UnityEngine.Object.Destroy(_levelPackDetailViewController._blurredPackArtwork);
                _levelPackDetailViewController._blurredPackArtwork = null;
            }

            var coverImage = sender is IPlaylist playlist ? playlist.Sprite : sender as Sprite;
            var sprite = coverImage != null ? coverImage : _levelPackDetailViewController._defaultCoverSprite;
            var texture = _levelPackDetailViewController._kawaseBlurRenderer.Blur(sprite.texture, KawaseBlurRendererSO.KernelSize.Kernel7, 2);
            _levelPackDetailViewController._blurredPackArtwork = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 256f, 0U, SpriteMeshType.FullRect, new Vector4(0f, 0f, 0f, 0f), false);
            _levelPackDetailViewController._packImage.sprite = sprite;
            _levelPackDetailViewController.ShowContent(LevelPackDetailViewController.ContentType.NonBuyable);
        }

        public void LevelCollectionUpdated(BeatmapLevelPack beatmapLevelPack, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            if (_selectedPlaylist != null)
            {
                playlistReferences.Remove(_selectedPlaylist);
                _selectedPlaylist.SpriteLoaded -= SelectedPlaylist_SpriteLoaded;
            }

            if (beatmapLevelPack is PlaylistLevelPack playlistLevelPack)
            {
                _selectedPlaylist = playlistLevelPack.playlist;
                if (playlistReferences.Add(_selectedPlaylist))
                {
                    _selectedPlaylist.SpriteLoaded += SelectedPlaylist_SpriteLoaded;
                }
            }
            else
            {
                _selectedPlaylist = null;
            }
        }
    }
}