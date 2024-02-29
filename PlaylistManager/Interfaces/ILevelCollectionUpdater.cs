namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionUpdater
    {
        public void LevelCollectionUpdated(BeatmapLevelPack annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager);
    }
}
