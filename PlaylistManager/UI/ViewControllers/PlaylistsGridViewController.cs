using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlaylistManager.UI
{
    public class PlaylistsGridViewController : IInitializable, IDisposable
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController;
        private AnnotatedBeatmapLevelCollectionsGridView annotatedBeatmapLevelCollectionsGridView;
        private AnnotatedBeatmapLevelCollectionsGridViewAnimator annotatedBeatmapLevelCollectionsGridViewAnimator;

        private readonly IVRPlatformHelper platformHelper;

        private GridScrollView gridScrollView;
        private Transform scrollBar;

        [UIComponent("scroll-view")]
        private ScrollView bsmlScrollView;

        public PlaylistsGridViewController(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController, IVRPlatformHelper platformHelper)
        {
            this.annotatedBeatmapLevelCollectionsTableViewController = annotatedBeatmapLevelCollectionsTableViewController;
            this.platformHelper = platformHelper;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsGridView = Accessors.AnnotatedBeatmapLevelCollectionsGridViewAccessor(ref annotatedBeatmapLevelCollectionsTableViewController);
            annotatedBeatmapLevelCollectionsGridViewAnimator = Accessors.AnnotatedBeatmapLevelCollectionsGridViewAnimatorAccessor(ref annotatedBeatmapLevelCollectionsGridView);

            RectTransform rectTransform = annotatedBeatmapLevelCollectionsGridView.gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(-10f, 0);
            rectTransform.localPosition = new Vector3(-5f, -7.5f, 0);

            AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init.OnGridViewInit += AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit;
        }

        public void Dispose()
        {
            AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init.OnGridViewInit -= AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit;
        }

        private void AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit()
        {
            AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init.OnGridViewInit -= AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.TableViewButtons.bsml"), annotatedBeatmapLevelCollectionsTableViewController.gameObject, this);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            annotatedBeatmapLevelCollectionsGridView.GetField<PageControl, AnnotatedBeatmapLevelCollectionsGridView>("_pageControl").gameObject.SetActive(false);

            scrollBar = bsmlScrollView.transform.Find("ScrollBar");
            scrollBar.SetParent(annotatedBeatmapLevelCollectionsTableViewController.transform);
            scrollBar.SetAsFirstSibling();
            Button pageUpButton = bsmlScrollView.GetField<Button, ScrollView>("_pageUpButton");
            Button pageDownButton = bsmlScrollView.GetField<Button, ScrollView>("_pageDownButton");
            VerticalScrollIndicator verticalScrollIndicator = bsmlScrollView.GetField<VerticalScrollIndicator, ScrollView>("_verticalScrollIndicator");
            GameObject.Destroy(bsmlScrollView.gameObject);

            annotatedBeatmapLevelCollectionsGridView.gameObject.SetActive(false);
            annotatedBeatmapLevelCollectionsGridView.gameObject.AddComponent<EventSystemListener>();
            gridScrollView = annotatedBeatmapLevelCollectionsGridView.gameObject.AddComponent<GridScrollView>();

            RectTransform viewport = annotatedBeatmapLevelCollectionsGridViewAnimator.GetField<RectTransform, AnnotatedBeatmapLevelCollectionsGridViewAnimator>("_viewportTransform");
            RectTransform content = annotatedBeatmapLevelCollectionsGridViewAnimator.GetField<RectTransform, AnnotatedBeatmapLevelCollectionsGridViewAnimator>("_contentTransform");

            gridScrollView.Init(viewport, content, pageUpButton, pageDownButton, verticalScrollIndicator);
            (gridScrollView as ScrollView).SetField("_platformHelper", platformHelper);
            annotatedBeatmapLevelCollectionsGridView.gameObject.SetActive(true);

            annotatedBeatmapLevelCollectionsTableViewController.didOpenBeatmapLevelCollectionsEvent += AnnotatedBeatmapLevelCollectionsGridView_didOpenAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsTableViewController.didCloseBeatmapLevelCollectionsEvent += AnnotatedBeatmapLevelCollectionsGridView_didCloseAnnotatedBeatmapLevelCollectionEvent;
            gridScrollView.enabled = false;
        }

        private void AnnotatedBeatmapLevelCollectionsGridView_didOpenAnnotatedBeatmapLevelCollectionEvent()
        {
            gridScrollView.OnHover();
        }

        private void AnnotatedBeatmapLevelCollectionsGridView_didCloseAnnotatedBeatmapLevelCollectionEvent()
        {
            gridScrollView.OnLeave();
        }
    }
}