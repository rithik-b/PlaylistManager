using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlaylistManager.UI
{
    public class TableViewButtonsController : IInitializable
    {
        private AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsTableViewController;

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
            AnnotatedBeatmapLevelCollectionsTableView annotatedBeatmapLevelCollectionsTableView = Accessors.AnnotatedBeatmapLevelCollectionsTableViewAccessor(ref annotatedBeatmapLevelCollectionsTableViewController);
            
            // Added RectMask2D to viewport to prevent the visual bug of playlist cells over buttons
            GameObject viewport = annotatedBeatmapLevelCollectionsTableView.transform.GetChild(0).gameObject;
            viewport.AddComponent<RectMask2D>().padding = new Vector4(0f, 0f, -1.9f, 0f);
            RectTransform rectTransform = annotatedBeatmapLevelCollectionsTableView.gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(0.71f, 1.0f);
            rectTransform.sizeDelta = new Vector2(-1.4f, 0);
            rectTransform.transform.localPosition = new Vector3(-10.55f, -6.5f, 0f);

            // Set buttons and scroll speed
            ScrollView scrollView = annotatedBeatmapLevelCollectionsTableView.gameObject.GetComponent<ScrollView>();
            scrollView.SetField("_pageUpButton", leftButton);
            scrollView.SetField("_pageDownButton", rightButton);
            scrollView.SetField("_joystickScrollSpeed", 60f * PluginConfig.Instance.PlaylistScrollSpeed);
        }
    }
}
