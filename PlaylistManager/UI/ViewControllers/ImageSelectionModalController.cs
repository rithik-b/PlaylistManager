using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace PlaylistManager.UI
{
    public class ImageSelectionModalController
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;

        private readonly string IMAGES_PATH = Path.Combine(PlaylistLibUtils.playlistManager.PlaylistPath, "CoverImages");
        private Dictionary<string, Sprite> coverImages;
        private bool parsed;

        public event Action<string> ImageSelectedEvent;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public ImageSelectionModalController(LevelPackDetailViewController levelPackDetailViewController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            Directory.CreateDirectory(IMAGES_PATH);
            File.Create(Path.Combine(IMAGES_PATH, ".plignore"));
            coverImages = new Dictionary<string, Sprite>();
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.ImageSelectionModal.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
                modalPosition = modalTransform.position;
                parsed = true;
            }
            modalTransform.position = modalPosition;
        }

        internal void ShowModal()
        {
            Parse();
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            ShowImages();
        }

        private void LoadImages()
        {
            foreach (var coverImage in coverImages)
            {
                if (!File.Exists(coverImage.Key))
                {
                    coverImages.Remove(coverImage.Key);
                }
            }

            string[] ext = { "jpg", "png" };
            IEnumerable<string> imageFiles = Directory.EnumerateFiles(IMAGES_PATH, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (var file in imageFiles)
            {
                if (!coverImages.ContainsKey(file))
                {
                    try
                    {
                        using (FileStream imageStream = File.Open(file, FileMode.Open))
                        {
                            byte[] imageBytes = new byte[imageStream.Length];
                            imageStream.Read(imageBytes, 0, (int)imageStream.Length);
                            Sprite coverImageSprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                            coverImages.Add(file, coverImageSprite);
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.Critical("Could not load " + file + "\nException message: " + e.Message);
                    }
                }
            }
        }

        private void ShowImages()
        {
            customListTableData.data.Clear();

            LoadImages();
            foreach (var coverImage in coverImages)
            {
                customListTableData.data.Add(new CustomCellInfo(Path.GetFileName(coverImage.Key), coverImage.Key, coverImage.Value));
            }

            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            customListTableData.tableView.ClearSelection();

            ImageSelectedEvent?.Invoke(customListTableData.data[index].subtext);

            parserParams.EmitEvent("close-modal");
        }
    }
}
