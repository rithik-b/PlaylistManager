using System;
using System.Collections.Generic;

namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionsTableUpdater
    {
        public event Action<IReadOnlyList<BeatmapLevelPack>, int> LevelCollectionTableViewUpdatedEvent;
    }
}
