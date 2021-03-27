using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Configuration;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlaylistManager.UI
{
    public class TableViewButtonsController : IInitializable
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController;

        public static readonly FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.Accessor AnnotatedBeatmapLevelCollectionsTableViewAccessor =
            FieldAccessor<AnnotatedBeatmapLevelCollectionsViewController, AnnotatedBeatmapLevelCollectionsTableView>.GetAccessor("_annotatedBeatmapLevelCollectionsTableView");

        [UIComponent("left-button")]
        private readonly Button leftButton;

        [UIComponent("right-button")]
        private readonly Button rightButton;

        public TableViewButtonsController(AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController)
        {
            this.annotatedBeatmapLevelCollectionsTableViewController = annotatedBeatmapLevelCollectionsTableViewController;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.TableViewButtons.bsml"), annotatedBeatmapLevelCollectionsTableViewController.gameObject, this);
            SetupButtons();
        }

        private void SetupButtons()
        {
            AnnotatedBeatmapLevelCollectionsTableView annotatedBeatmapLevelCollectionsTableView = AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsTableViewController);
            
            // Added RectMask2D to viewport to prevent the visual bug of playlist cells over buttons
            GameObject viewport = annotatedBeatmapLevelCollectionsTableView.transform.GetChild(0).gameObject;
            viewport.AddComponent<RectMask2D>().padding = new Vector4(0f, 0f, -1.9f, 0f);
            RectTransform rectTransform = annotatedBeatmapLevelCollectionsTableView.gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(0.78f, 1.0f);

            // Set buttons and scroll speed
            ScrollView scrollView = annotatedBeatmapLevelCollectionsTableView.gameObject.GetComponent<ScrollView>();
            FieldAccessor<ScrollView, Button>.Set(ref scrollView, "_pageUpButton", leftButton);
            FieldAccessor<ScrollView, Button>.Set(ref scrollView, "_pageDownButton", rightButton);
            FieldAccessor<ScrollView, float>.Set(ref scrollView, "_joystickScrollSpeed", 60f * PluginConfig.Instance.PlaylistScrollSpeed);

            // Tried doing it in BSML anchor pos and did not work
            leftButton.transform.localPosition = new Vector3(-51f, 0f, 0f);
            rightButton.transform.localPosition = new Vector3(33f, 0f, 0f);
            rectTransform.transform.localPosition = new Vector3(-10f, -6.5f, 0f);
        }
    }
}
