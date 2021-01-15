using BeatSaberPlaylistsLib.Types;
using IPA.Utilities;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using Zenject;

namespace PlaylistManager.UI
{
    class SpriteUpdateUI : IInitializable, IDisposable
    {
        private AnnotatedBeatmapLevelCollectionTableCell annotatedBeatmapLevelCollectionTableCell;
        public static readonly ConditionalWeakTable<IDeferredSpriteLoad, AnnotatedBeatmapLevelCollectionTableCell> EventTable = new ConditionalWeakTable<IDeferredSpriteLoad, AnnotatedBeatmapLevelCollectionTableCell>();
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, Image>.Accessor CoverImageAccessor
            = FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, Image>.GetAccessor("_coverImage");
        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, IAnnotatedBeatmapLevelCollection>.Accessor BeatmapCollectionAccessor
             = FieldAccessor<AnnotatedBeatmapLevelCollectionTableCell, IAnnotatedBeatmapLevelCollection>.GetAccessor("_annotatedBeatmapLevelCollection");

        SpriteUpdateUI(AnnotatedBeatmapLevelCollectionTableCell annotatedBeatmapLevelCollectionTableCell)
        {
            this.annotatedBeatmapLevelCollectionTableCell = annotatedBeatmapLevelCollectionTableCell;
        }

        public void Initialize()
        {
            var annotatedBeatmapLevelCollection = BeatmapCollectionAccessor(ref annotatedBeatmapLevelCollectionTableCell);
            if (annotatedBeatmapLevelCollection is IDeferredSpriteLoad)
            {
                ((IDeferredSpriteLoad)annotatedBeatmapLevelCollection).SpriteLoaded += SpriteUpdateUI_SpriteLoaded;
            }
        }

        public void Dispose()
        {
            var annotatedBeatmapLevelCollection = BeatmapCollectionAccessor(ref annotatedBeatmapLevelCollectionTableCell);
            if (annotatedBeatmapLevelCollection is IDeferredSpriteLoad)
            {
                ((IDeferredSpriteLoad)annotatedBeatmapLevelCollection).SpriteLoaded -= SpriteUpdateUI_SpriteLoaded;
            }
        }

        private void SpriteUpdateUI_SpriteLoaded(object sender, EventArgs e)
        {
            CoverImageAccessor(ref annotatedBeatmapLevelCollectionTableCell).sprite = ((IDeferredSpriteLoad)sender).Sprite;
        }
    }
}
