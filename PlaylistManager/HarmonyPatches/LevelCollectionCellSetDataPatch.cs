using System;
using UnityEngine.UI;
using BeatSaberPlaylistsLib.Types;
using System.Runtime.CompilerServices;
using PlaylistManager.Utilities;
using HMUI;
using PlaylistManager.Configuration;
using SiraUtil.Affinity;

/*
 * Original Author: Auros
 * Taken from PlaylistCore
 */

namespace PlaylistManager.HarmonyPatches
{
    internal class LevelCollectionCellSetDataPatch : IAffinity
    {
        private readonly ConditionalWeakTable<IDeferredSpriteLoad, AnnotatedBeatmapLevelCollectionCell> eventTable = new ConditionalWeakTable<IDeferredSpriteLoad, AnnotatedBeatmapLevelCollectionCell>();
        private readonly HoverHintController hoverHintController;

        public LevelCollectionCellSetDataPatch(HoverHintController hoverHintController)
        {
            this.hoverHintController = hoverHintController;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.SetData))]
        private void Patch(AnnotatedBeatmapLevelCollectionCell __instance, ref IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, ref Image ____coverImage)
        {
            AnnotatedBeatmapLevelCollectionCell cell = __instance;
            if (annotatedBeatmapLevelCollection is IDeferredSpriteLoad deferredSpriteLoad)
            {
                if (deferredSpriteLoad.SpriteWasLoaded)
                {
#if DEBUG
                    //Plugin.Log.Debug($"Sprite was already loaded for {(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName}");
#endif
                }
                if (eventTable.TryGetValue(deferredSpriteLoad, out AnnotatedBeatmapLevelCollectionCell existing))
                {
                    eventTable.Remove(deferredSpriteLoad);
                }
                eventTable.Add(deferredSpriteLoad, cell);
                deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                deferredSpriteLoad.SpriteLoaded += OnSpriteLoaded;
            }

            if (PluginConfig.Instance.PlaylistHoverHints)
            {
                HoverHint hoverHint = __instance.GetComponent<HoverHint>();

                if (hoverHint == null)
                {
                    hoverHint = __instance.gameObject.AddComponent<HoverHint>();
                    Accessors.HoverHintControllerAccessor(ref hoverHint) = hoverHintController;
                }

                hoverHint.text = annotatedBeatmapLevelCollection.collectionName;
            }
        }

        private void OnSpriteLoaded(object sender, EventArgs e)
        {
            if (sender is IDeferredSpriteLoad deferredSpriteLoad)
            {
                if (eventTable.TryGetValue(deferredSpriteLoad, out AnnotatedBeatmapLevelCollectionCell tableCell))
                {
                    if (tableCell == null)
                    {
                        deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                        return;
                    }

                    IAnnotatedBeatmapLevelCollection collection = Accessors.BeatmapCollectionAccessor(ref tableCell);
                    if (collection == deferredSpriteLoad)
                    {
#if DEBUG
                        //Plugin.Log.Debug($"Updating image for {collection.collectionName}");
#endif
                        Accessors.CoverImageAccessor(ref tableCell).sprite = deferredSpriteLoad.Sprite;
                    }
                    else
                    {
                        //Plugin.Log.Warn($"Collection '{collection.collectionName}' is not {(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName}");
                        eventTable.Remove(deferredSpriteLoad);
                        deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                    }
                }
                else
                {
                    //Plugin.Log.Warn($"{(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName} is not in the EventTable.");
                    deferredSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                }
            }
            else
            {
                //Plugin.Log.Warn($"Wrong sender type for deferred sprite load: {sender?.GetType().Name ?? "<NULL>"}");
            }
        }
    }
}