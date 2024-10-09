﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IPA.Loader;
using SiraUtil.Zenject;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace PlaylistManager.UI
{
    public class ImageSelectionModalController : NotifiableBase
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;
        private readonly PluginMetadata pluginMetadata;
        private readonly BSMLParser bsmlParser;

        private readonly string IMAGES_PATH = Path.Combine(PlaylistLibUtils.playlistManager.PlaylistPath, "CoverImages");
        private readonly Sprite playlistManagerIcon;
        private readonly Dictionary<string, CoverImage> coverImages;
        private bool parsed;
        private int selectedIndex;

        public event Action<byte[]> ImageSelectedEvent;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public ImageSelectionModalController(LevelPackDetailViewController levelPackDetailViewController, PopupModalsController popupModalsController, UBinder<Plugin, PluginMetadata> pluginMetadata, BSMLParser bsmlParser)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;
            this.pluginMetadata = pluginMetadata.Value;
            this.bsmlParser = bsmlParser;

            // Have to do this in case directory perms are not given
            try
            {
                Directory.CreateDirectory(IMAGES_PATH);
                File.Create(Path.Combine(IMAGES_PATH, ".plignore"));
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"Could not make images path.\nExcepton:{e.Message}");
            }

            coverImages = new Dictionary<string, CoverImage>();
            playlistManagerIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.DefaultIcon.png");
            parsed = false;
        }

        private void Parse()
        {
            if (!parsed)
            {
                bsmlParser.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(pluginMetadata.Assembly, "PlaylistManager.UI.Views.ImageSelectionModal.bsml"), levelPackDetailViewController._detailWrapper.gameObject, this);
                modalPosition = modalTransform.position;
            }
            modalTransform.position = modalPosition;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            parsed = true;
            modalView._animateParentCanvas = false;
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
            var imageFiles = Directory.EnumerateFiles(IMAGES_PATH, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (var file in imageFiles)
            {
                if (!coverImages.ContainsKey(file))
                {
                    coverImages.Add(file, new CoverImage(file));
                }
            }
        }

        private async void ShowImages(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => customListTableData.Data.Clear());

            IsLoading = true;

            // Add clear image
            customListTableData.Data.Add(new CustomCellInfo("Clear Icon", "Clear", await PlaylistLibUtils.GeneratePlaylistIcon(playlist)));

            // Add default image
            customListTableData.Data.Add(new CustomCellInfo("PlaylistManager Icon", "Default", playlistManagerIcon));

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
                    customListTableData.Data.Add(new CustomCellInfo(Path.GetFileName(coverImage.Key), coverImage.Key, coverImage.Value.Sprite));
                }
            }

            await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => customListTableData.TableView.ReloadData());
            customListTableData.TableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            _ = ViewControllerMonkeyCleanup();
        }

        private void CoverImage_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is CoverImage coverImage)
            {
                if (coverImage.SpriteWasLoaded)
                {
                    customListTableData.Data.Add(new CustomCellInfo(Path.GetFileName(coverImage.Path), coverImage.Path, coverImage.Sprite));
                    customListTableData.TableView.ReloadDataKeepingPosition();

                    if (customListTableData.Data.Count == 4)
                    {
                        customListTableData.TableView.AddCellToReusableCells(customListTableData.TableView.dataSource.CellForIdx(customListTableData.TableView, 3));
                    }
                    _ = ViewControllerMonkeyCleanup();
                }
                coverImage.SpriteLoaded -= CoverImage_SpriteLoaded;
            }
        }

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            customListTableData.TableView.ClearSelection();
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
                using (var imageStream = pluginMetadata.Assembly.GetManifestResourceStream("PlaylistManager.Icons.DefaultIcon.png"))
                {
                    var imageBytes = new byte[imageStream.Length];
                    imageStream.Read(imageBytes, 0, (int)imageStream.Length);
                    ImageSelectedEvent?.Invoke(imageBytes);
                    parserParams.EmitEvent("close-modal");
                }
            }
            else
            {
                var selectedImagePath = customListTableData.Data[selectedIndex].Subtext;
                try
                {
                    using (var imageStream = File.Open(selectedImagePath, FileMode.Open))
                    {
                        var imageBytes = new byte[imageStream.Length];
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
            await SiraUtil.Extras.Utilities.PauseChamp;
            var imageViews = customListTableData.TableView.GetComponentsInChildren<ImageView>(true);
            for (var i = 0; i < imageViews.Length; i++)
            {
                imageViews[i]._skew = 0f;
            }
            IsLoading = false;
        }

        private bool _isLoading;

        [UIValue("is-loading")]
        private bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotLoading));
            }
        }

        [UIValue("is-not-loading")]
        private bool IsNotLoading => !IsLoading;
    }
}
