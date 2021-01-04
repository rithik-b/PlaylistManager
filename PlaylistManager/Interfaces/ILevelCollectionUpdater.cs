using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionUpdater
    {
        void LevelCollectionUpdated(IAnnotatedBeatmapLevelCollection beatmapLevelCollection);
    }
}
