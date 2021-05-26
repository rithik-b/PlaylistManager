using System;
using HarmonyLib;
using UnityEngine.UI;
using BeatSaberPlaylistsLib.Types;
using System.Runtime.CompilerServices;
using PlaylistManager.Utilities;

/*
 * Original Author: Auros
 * Taken from PlaylistCore
 */

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionTableCell), nameof(AnnotatedBeatmapLevelCollectionTableCell.SetData))]
    public class AnnotatedBeatmapLevelCollectionTableCell_SetData
    {
        public static readonly ConditionalWeakTable<IDeferredSpriteLoad, AnnotatedBeatmapLevelCollectionTableCell> EventTable = new ConditionalWeakTable<IDeferredSpriteLoad, AnnotatedBeatmapLevelCollectionTableCell>();

        static void Postfix(AnnotatedBeatmapLevelCollectionTableCell __instance, ref IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, ref Image ____coverImage)
        {
            AnnotatedBeatmapLevelCollectionTableCell cell = __instance;
            if (annotatedBeatmapLevelCollection is IDeferredSpriteLoad deferredSpriteLoad)
            {
                if (deferredSpriteLoad.SpriteWasLoaded)
                {
#if DEBUG
                    Plugin.Log.Debug($"Sprite was already loaded for {(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName}");
#endif
                }
                if (EventTable.TryGetValue(deferredSpriteLoad, out AnnotatedBeatmapLevelCollectionTableCell existing))
                {
                    EventTable.Remove(deferredSpriteLoad);
                }
                EventTable.Add(deferredSpriteLoad, cell);
                deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                deferredSpriteLoad.SpriteLoaded += OnSpriteLoaded;
            }
        }

        public static void OnSpriteLoaded(object sender, EventArgs e)
        {
            if (sender is IDeferredSpriteLoad deferredSpriteLoad)
            {
                if (EventTable.TryGetValue(deferredSpriteLoad, out AnnotatedBeatmapLevelCollectionTableCell tableCell))
                {
                    IAnnotatedBeatmapLevelCollection collection = Accessors.BeatmapCollectionAccessor(ref tableCell);
                    if (collection == deferredSpriteLoad)
                    {
#if DEBUG
                        Plugin.Log.Debug($"Updating image for {collection.collectionName}");
#endif
                        Accessors.CoverImageAccessor(ref tableCell).sprite = deferredSpriteLoad.Sprite;
                    }
                    else
                    {
                        Plugin.Log.Warn($"Collection '{collection.collectionName}' is not {(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName}");
                        EventTable.Remove(deferredSpriteLoad);
                        deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                    }
                }
                else
                {
                    Plugin.Log.Warn($"{(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName} is not in the EventTable.");
                    deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                }
            }
            else
            {
                Plugin.Log.Warn($"Wrong sender type for deferred sprite load: {sender?.GetType().Name ?? "<NULL>"}");
            }
        }
    }
}