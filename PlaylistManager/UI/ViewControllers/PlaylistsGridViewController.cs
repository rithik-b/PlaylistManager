using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

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

        [UIComponent("vertical")]
        private RectTransform vertical;

        public PlaylistsGridViewController(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController, IVRPlatformHelper platformHelper)
        {
            this.annotatedBeatmapLevelCollectionsTableViewController = annotatedBeatmapLevelCollectionsTableViewController;
            this.platformHelper = platformHelper;
        }

        public void Initialize()
        {
            annotatedBeatmapLevelCollectionsGridView = Accessors.AnnotatedBeatmapLevelCollectionsGridViewAccessor(ref annotatedBeatmapLevelCollectionsTableViewController);
            annotatedBeatmapLevelCollectionsGridViewAnimator = Accessors.GridViewAnimatorAccessor(ref annotatedBeatmapLevelCollectionsGridView);

            // Removing last column of GridView to make space for our scroller
            RectTransform rectTransform = annotatedBeatmapLevelCollectionsGridView.gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(-10f, 0);
            rectTransform.localPosition = new Vector3(-5f, -7.5f, 0);

            AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init.OnGridViewInit += AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit;
        }

        public void Dispose()
        {
            AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init.OnGridViewInit -= AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit;
            annotatedBeatmapLevelCollectionsTableViewController.didOpenBeatmapLevelCollectionsEvent -= AnnotatedBeatmapLevelCollectionsGridView_didOpenAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsTableViewController.didCloseBeatmapLevelCollectionsEvent -= AnnotatedBeatmapLevelCollectionsGridView_didCloseAnnotatedBeatmapLevelCollectionEvent;
        }

        private void AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit()
        {
            AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init.OnGridViewInit -= AnnotatedBeatmapLevelCollectionsGridViewAnimator_Init_OnGridViewInit;
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.PlaylistsGridView.bsml"), annotatedBeatmapLevelCollectionsTableViewController.gameObject, this);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            // Removing PageControl as it looks really ugly with a lot of playlists, scroll indicator replaces this
            Accessors.PageControlAccessor(ref annotatedBeatmapLevelCollectionsGridView).gameObject.SetActive(false);

            // Getting Viewport and Content
            RectTransform viewport = Accessors.GridViewportAccessor(ref annotatedBeatmapLevelCollectionsGridViewAnimator);
            RectTransform content = Accessors.GridContentAccessor(ref annotatedBeatmapLevelCollectionsGridViewAnimator);
            content.localPosition = Vector3.zero;

            // Breaking up ScrollBar from ScrollView
            scrollBar = bsmlScrollView.transform.Find("ScrollBar");
            scrollBar.SetParent(vertical);
            vertical.SetParent(viewport);
            Button pageUpButton = Accessors.PageUpAccessor(ref bsmlScrollView);
            Button pageDownButton = Accessors.PageDownAccessor(ref bsmlScrollView);
            VerticalScrollIndicator verticalScrollIndicator = Accessors.ScrollIndicatorAccessor(ref bsmlScrollView);
            Object.Destroy(bsmlScrollView.gameObject);
            scrollBar.gameObject.SetActive(false);

            // Adding GridScrollView to GridView
            annotatedBeatmapLevelCollectionsGridView.gameObject.SetActive(false);
            annotatedBeatmapLevelCollectionsGridView.gameObject.AddComponent<EventSystemListener>();
            gridScrollView = annotatedBeatmapLevelCollectionsGridView.gameObject.AddComponent<GridScrollView>();
            ScrollView scrollView = gridScrollView;

            // Initializing GridScrollView
            gridScrollView.Init(viewport, content, pageUpButton, pageDownButton, verticalScrollIndicator);
            Accessors.PlatformHelperAccessor(ref scrollView) = platformHelper;
            annotatedBeatmapLevelCollectionsGridView.gameObject.SetActive(true);
            gridScrollView.enabled = false;

            // Subbing to events
            annotatedBeatmapLevelCollectionsTableViewController.didOpenBeatmapLevelCollectionsEvent += AnnotatedBeatmapLevelCollectionsGridView_didOpenAnnotatedBeatmapLevelCollectionEvent;
            annotatedBeatmapLevelCollectionsTableViewController.didCloseBeatmapLevelCollectionsEvent += AnnotatedBeatmapLevelCollectionsGridView_didCloseAnnotatedBeatmapLevelCollectionEvent;
        }

        private void AnnotatedBeatmapLevelCollectionsGridView_didOpenAnnotatedBeatmapLevelCollectionEvent()
        {
            scrollBar.gameObject.SetActive(true);
            gridScrollView.OnHover();
        }

        private void AnnotatedBeatmapLevelCollectionsGridView_didCloseAnnotatedBeatmapLevelCollectionEvent()
        {
            scrollBar.gameObject.SetActive(false);
            gridScrollView.OnLeave();
        }
    }
}