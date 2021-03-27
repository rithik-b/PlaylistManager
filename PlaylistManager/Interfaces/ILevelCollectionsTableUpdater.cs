using System;

namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionsTableUpdater
    {
        public event Action<IAnnotatedBeatmapLevelCollection[], int> LevelCollectionTableViewUpdatedEvent;
    }
}
