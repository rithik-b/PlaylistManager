namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionUpdater
    {
        public void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager);
    }
}
