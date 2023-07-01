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

namespace PlaylistManager.AffinityPatches
{
    internal class LevelCollectionCellSetDataPatch : IAffinity
    {
        private readonly ConditionalWeakTable<IStagedSpriteLoad, AnnotatedBeatmapLevelCollectionCell> eventTable = new();
        private readonly HoverHintController hoverHintController;

        public LevelCollectionCellSetDataPatch(HoverHintController hoverHintController)
        {
            this.hoverHintController = hoverHintController;
        }

        [AffinityPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.SetData))]
        private void Patch(AnnotatedBeatmapLevelCollectionCell __instance, ref IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection, ref Image ____coverImage)
        {
            var cell = __instance;
            if (annotatedBeatmapLevelCollection is IStagedSpriteLoad stagedSpriteLoad)
            {
                if (stagedSpriteLoad.SmallSpriteWasLoaded)
                {
#if DEBUG
                    //Plugin.Log.Debug($"Sprite was already loaded for {(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName}");
#endif
                }
                if (eventTable.TryGetValue(stagedSpriteLoad, out var existing))
                {
                    eventTable.Remove(stagedSpriteLoad);
                }
                eventTable.Add(stagedSpriteLoad, cell);
                stagedSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                stagedSpriteLoad.SpriteLoaded += OnSpriteLoaded;
            }

            if (PluginConfig.Instance.PlaylistHoverHints)
            {
                var hoverHint = __instance.GetComponent<HoverHint>();

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
            if (sender is IStagedSpriteLoad stagedSpriteLoad)
            {
                if (eventTable.TryGetValue(stagedSpriteLoad, out var tableCell))
                {
                    if (tableCell == null)
                    {
                        stagedSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                        return;
                    }

                    var collection = tableCell._annotatedBeatmapLevelCollection;
                    if (collection == stagedSpriteLoad)
                    {
#if DEBUG
                        //Plugin.Log.Debug($"Updating image for {collection.collectionName}");
#endif
                        tableCell._coverImage.sprite = stagedSpriteLoad.SmallSprite;
                    }
                    else
                    {
                        //Plugin.Log.Warn($"Collection '{collection.collectionName}' is not {(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName}");
                        eventTable.Remove(stagedSpriteLoad);
                        stagedSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                    }
                }
                else
                {
                    //Plugin.Log.Warn($"{(deferredSpriteLoad as IAnnotatedBeatmapLevelCollection).collectionName} is not in the EventTable.");
                    stagedSpriteLoad.SpriteLoaded -= OnSpriteLoaded;
                }
            }
            else
            {
                //Plugin.Log.Warn($"Wrong sender type for deferred sprite load: {sender?.GetType().Name ?? "<NULL>"}");
            }
        }
    }
}