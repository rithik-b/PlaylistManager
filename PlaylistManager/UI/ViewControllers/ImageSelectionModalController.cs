using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace PlaylistManager.UI
{
    public class ImageSelectionModalController : INotifyPropertyChanged
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;

        private readonly string IMAGES_PATH = Path.Combine(PlaylistLibUtils.playlistManager.PlaylistPath, "CoverImages");
        private readonly Sprite playlistManagerIcon;
        private readonly Dictionary<string, CoverImage> coverImages;
        private bool parsed;
        private int selectedIndex;

        public event Action<byte[]> ImageSelectedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public ImageSelectionModalController(LevelPackDetailViewController levelPackDetailViewController, PopupModalsController popupModalsController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;
            Directory.CreateDirectory(IMAGES_PATH);
            File.Create(Path.Combine(IMAGES_PATH, ".plignore"));
            coverImages = new Dictionary<string, CoverImage>();
            playlistManagerIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.Logo.png");
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.ImageSelectionModal.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
                modalPosition = modalTransform.position;
                FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", false);
                parsed = true;
            }
            modalTransform.position = modalPosition;
        }

        internal void ShowModal(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            Parse();
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            ShowImages(playlist);
        }

        private void LoadImages()
        {
            foreach (var imageToDelete in coverImages.Where(coverImage => !File.Exists(coverImage.Key)).ToList())
            {
                coverImages.Remove(imageToDelete.Key);
            }

            string[] ext = { "jpg", "png" };
            IEnumerable<string> imageFiles = Directory.EnumerateFiles(IMAGES_PATH, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (var file in imageFiles)
            {
                if (!coverImages.ContainsKey(file))
                {
                    coverImages.Add(file, new CoverImage(file));
                }
            }
        }

        private void ShowImages(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            customListTableData.data.Clear();

            // Add clear image
            customListTableData.data.Add(new CustomCellInfo("Clear Icon", "Clear", PlaylistLibUtils.GeneratePlaylistIcon(playlist)));

            // Add default image
            customListTableData.data.Add(new CustomCellInfo("PlaylistManager Icon", "Default", playlistManagerIcon));

            LoadImages();
            foreach (var coverImage in coverImages)
            {
                if(!coverImage.Value.SpriteWasLoaded && !coverImage.Value.Blacklist)
                {
                    coverImage.Value.SpriteLoaded += CoverImage_SpriteLoaded;
                    _ = coverImage.Value.Sprite;
                }
                else if(coverImage.Value.SpriteWasLoaded)
                {
                    customListTableData.data.Add(new CustomCellInfo(Path.GetFileName(coverImage.Key), coverImage.Key, coverImage.Value.Sprite));
                }
            }
            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            _ = ViewControllerMonkeyCleanup();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpButtonEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownButtonEnabled)));
        }

        private void CoverImage_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is CoverImage coverImage)
            {
                if (coverImage.SpriteWasLoaded)
                {
                    customListTableData.data.Add(new CustomCellInfo(Path.GetFileName(coverImage.Path), coverImage.Path, coverImage.Sprite));
                    customListTableData.tableView.ReloadDataKeepingPosition();

                    if (customListTableData.data.Count == 4)
                    {
                        customListTableData.tableView.AddCellToReusableCells(customListTableData.tableView.dataSource.CellForIdx(customListTableData.tableView, 3));
                    }
                    _ = ViewControllerMonkeyCleanup();

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpButtonEnabled)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownButtonEnabled)));
                }
                coverImage.SpriteLoaded -= CoverImage_SpriteLoaded;
            }
        }

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            customListTableData.tableView.ClearSelection();
            selectedIndex = index;
            popupModalsController.ShowYesNoModal(modalTransform, "Are you sure you want to change the image of the playlist? This cannot be reverted.", ChangeImage, animateParentCanvas: false);
        }

        private void ChangeImage()
        {
            if (selectedIndex == 0)
            {
                ImageSelectedEvent?.Invoke(null);
                parserParams.EmitEvent("close-modal");
            }
            else if (selectedIndex == 1)
            {
                using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.Logo.png"))
                {
                    byte[] imageBytes = new byte[imageStream.Length];
                    imageStream.Read(imageBytes, 0, (int)imageStream.Length);
                    ImageSelectedEvent?.Invoke(imageBytes);
                    parserParams.EmitEvent("close-modal");
                }
            }
            else
            {
                string selectedImagePath = customListTableData.data[selectedIndex].subtext;
                try
                {
                    using (FileStream imageStream = File.Open(selectedImagePath, FileMode.Open))
                    {
                        byte[] imageBytes = new byte[imageStream.Length];
                        imageStream.Read(imageBytes, 0, (int)imageStream.Length);
                        ImageSelectedEvent?.Invoke(imageBytes);
                        parserParams.EmitEvent("close-modal");
                    }
                }
                catch (Exception e)
                {
                    popupModalsController.ShowOkModal(modalTransform, "There was an error loading this image. Check logs for more details.", null, animateParentCanvas: false);
                    Plugin.Log.Critical("Could not load " + selectedImagePath + "\nException message: " + e.Message);
                }
            }
        }

        private async Task ViewControllerMonkeyCleanup()
        {
            await SiraUtil.Utilities.PauseChamp;
            ImageView[] imageViews = customListTableData.tableView.GetComponentsInChildren<ImageView>(true);
            foreach (var imageView in imageViews)
            {
                imageView.SetField("_skew", 0f);
            }
        }

        [UIValue("up-button-enabled")]
        private bool UpButtonEnabled
        {
            get => customListTableData != null && customListTableData.data.Count > 3;
        }

        [UIValue("down-button-enabled")]
        private bool DownButtonEnabled
        {
            get => customListTableData != null && customListTableData.data.Count > 3;
        }
    }
}
