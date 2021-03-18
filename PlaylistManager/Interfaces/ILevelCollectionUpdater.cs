namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionUpdater
    {
        void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, BeatSaberPlaylistsLib.PlaylistManager parentManager);
    }
}
