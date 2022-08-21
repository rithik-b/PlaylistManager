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
using System.Threading.Tasks;
using BeatSaberPlaylistsLib;
using UnityEngine;
using Zenject;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace PlaylistManager.UI
{
    public class ImageSelectionModalController : NotifiableBase, IInitializable
    {
        private readonly LevelPackDetailViewController levelPackDetailViewController;
        private readonly PopupModalsController popupModalsController;

        private readonly string imagesPath = Path.Combine(BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.PlaylistPath, "CoverImages");
        private readonly Sprite playlistManagerIcon;
        private readonly Dictionary<string, Sprite> coverImages;
        private bool parsed;
        private int selectedIndex;
        
        public event Action<byte[]?>? ImageSelectedEvent;

        [UIComponent("list")]
        private readonly CustomListTableData customListTableData = null!;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform = null!;

        [UIComponent("modal")]
        private ModalView modalView = null!;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams = null!;

        public ImageSelectionModalController(LevelPackDetailViewController levelPackDetailViewController, PopupModalsController popupModalsController)
        {
            this.levelPackDetailViewController = levelPackDetailViewController;
            this.popupModalsController = popupModalsController;

            coverImages = new Dictionary<string, Sprite>();
            playlistManagerIcon = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("PlaylistManager.Icons.DefaultCover.png");
            parsed = false;
        }
        
        public void Initialize()
        {
            // Have to do this in case directory perms are not given
            try
            {
                Directory.CreateDirectory(imagesPath);
                File.Create(Path.Combine(imagesPath, ".plignore"));
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"Could not make images path.\nExcepton:{e.Message}");
            }
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "PlaylistManager.UI.Views.ImageSelectionModal.bsml"), levelPackDetailViewController.transform.Find("Detail").gameObject, this);
                modalPosition = modalTransform.position;
            }
            modalTransform.position = modalPosition;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            parsed = true;
            Accessors.AnimateCanvasAccessor(ref modalView) = false;
        }

        internal void ShowModal(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            Parse();
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            _ = ShowImages(playlist).ConfigureAwait(false);
        }

        private IEnumerable<string> GetImagePaths()
        {
            foreach (var imageToDelete in coverImages.Where(coverImage => !File.Exists(coverImage.Key)).ToList())
            {
                coverImages.Remove(imageToDelete.Key);
            }

            string[] ext = { "jpg", "png" };
            var imagePaths = Directory.EnumerateFiles(imagesPath, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            return imagePaths;
        }

        private async Task ShowImages(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => customListTableData.data.Clear());

            IsLoading = true;
            
            // Add clear image
            customListTableData.data.Add(new CustomCellInfo("Clear Icon", "Clear", await PlaylistLibUtils.GeneratePlaylistIcon(playlist)));

            // Add default image
            customListTableData.data.Add(new CustomCellInfo("PlaylistManager Icon", "Default", playlistManagerIcon));

            var imagePaths = GetImagePaths();
            foreach (var imagePath in imagePaths)
            {
                if (coverImages.TryGetValue(imagePath, out var outSprite))
                {
                    customListTableData.data.Add(new CustomCellInfo(Path.GetFileName(imagePath), imagePath, outSprite));
                }
                else
                {
                    try
                    {
                        using var imageStream = File.Open(imagePath, FileMode.Open);
                        var imageBytes = imageStream.ToArray();
                        await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                        {
                            var sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                            customListTableData.data.Add(new CustomCellInfo(Path.GetFileName(imagePath), imagePath, sprite));
                            coverImages[imagePath] = sprite;
                        });
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.Error("Could not load " + imagePath + "\nException message: " + e.Message);
                    }
                    await Task.Delay(100);
                }
            }
            
            await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => customListTableData.tableView.ReloadData());
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            _ = ViewControllerMonkeyCleanup();
        }

        [UIAction("select-cell")]
        private void OnCellSelect(TableView tableView, int index)
        {
            customListTableData.tableView.ClearSelection();
            selectedIndex = index;
            popupModalsController.ShowYesNoModal(modalTransform,
                "Are you sure you want to change the image of the playlist? This cannot be reverted.",
                () => _ = ChangeImage().ConfigureAwait(false), animateParentCanvas: false);
        }

        private Task ChangeImage()
        {
            if (selectedIndex == 0)
            {
                ImageSelectedEvent?.Invoke(null);
                parserParams.EmitEvent("close-modal");
            }
            else if (selectedIndex == 1)
            {
                using var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.DefaultCover.png");
                var imageBytes = imageStream!.ToArray();
                ImageSelectedEvent?.Invoke(imageBytes);
                parserParams.EmitEvent("close-modal");
            }
            else
            {
                var selectedImagePath = customListTableData.data[selectedIndex].subtext;
                try
                {
                    using var imageStream = File.Open(selectedImagePath, FileMode.Open);
                    var imageBytes = imageStream.ToArray();
                    ImageSelectedEvent?.Invoke(imageBytes);
                    parserParams.EmitEvent("close-modal");
                }
                catch (Exception e)
                {
                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        popupModalsController.ShowOkModal(modalTransform,
                            "There was an error loading this image. Check logs for more details.",
                            null, animateParentCanvas: false);
                    });
                    Plugin.Log.Error("Could not load " + selectedImagePath + "\nException message: " + e.Message);
                }
            }
            
            return Task.CompletedTask;
        }

        private async Task ViewControllerMonkeyCleanup()
        {
            await SiraUtil.Extras.Utilities.PauseChamp;
            var imageViews = customListTableData.tableView.GetComponentsInChildren<ImageView>(true);
            for (var i = 0; i < imageViews.Length; i++)
            {
                Accessors.SkewAccessor(ref imageViews[i]) = 0f;
            }
            IsLoading = false;
        }

        private bool isLoading;

        [UIValue("is-loading")]
        private bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotLoading));
            }
        }

        [UIValue("is-not-loading")]
        private bool IsNotLoading => !IsLoading;
    }
}
