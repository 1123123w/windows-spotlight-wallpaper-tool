using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WindowsSpotlightWallpaperTool;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private const string LandscapeFilter = "宽屏";
    private const string PortraitFilter = "竖屏";
    private const string SquareLikeFilter = "近似方形";

    private readonly SpotlightImageService _spotlightImageService = new();
    private readonly ImageExportService _imageExportService = new();
    private readonly List<SpotlightImage> _allImages = new();
    private SpotlightImage? _selectedImage;
    private BitmapSource? _previewImage;
    private string _selectedAspectFilter = LandscapeFilter;
    private string _selectedExportFormat = "JPEG";
    private string _scanSummary = "尚未扫描。";
    private string _imageCountText = "0 张图片";
    private string _statusText = "准备就绪。";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ExportFormats.Add("JPEG");
        ExportFormats.Add("PNG");
        if (_imageExportService.IsAvifSupported)
        {
            ExportFormats.Add("AVIF");
        }

        Loaded += (_, _) => RefreshImages();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<SpotlightImage> Images { get; } = new();
    public ObservableCollection<string> ExportFormats { get; } = new();
    public IReadOnlyList<string> AspectFilters { get; } = [LandscapeFilter, PortraitFilter, SquareLikeFilter];

    public SpotlightImage? SelectedImage
    {
        get => _selectedImage;
        set
        {
            if (SetField(ref _selectedImage, value))
            {
                LoadSelectedPreview();
            }
        }
    }

    public BitmapSource? PreviewImage
    {
        get => _previewImage;
        private set => SetField(ref _previewImage, value);
    }

    public string SelectedAspectFilter
    {
        get => _selectedAspectFilter;
        set
        {
            if (SetField(ref _selectedAspectFilter, value))
            {
                ApplyImageFilter();
            }
        }
    }

    public string SelectedExportFormat
    {
        get => _selectedExportFormat;
        set => SetField(ref _selectedExportFormat, value);
    }

    public string ScanSummary
    {
        get => _scanSummary;
        private set => SetField(ref _scanSummary, value);
    }

    public string ImageCountText
    {
        get => _imageCountText;
        private set => SetField(ref _imageCountText, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public string ProjectInfo => "作者：1123123w · GitHub: https://github.com/1123123w/windows-spotlight-wallpaper-tool";

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshImages();
    }

    private void ImagesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        LoadSelectedPreview();
    }

    private void AspectFilterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ApplyImageFilter();
    }

    private void ImageCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateImageCountText();
    }

    private void SaveSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        var checkedImages = _allImages.Where(image => image.IsChecked).ToList();
        if (checkedImages.Count == 0)
        {
            MessageBox.Show(this, "请先勾选至少一张图片。", "未勾选图片", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new OpenFolderDialog
        {
            Title = "选择保存文件夹",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var format = GetSelectedExportFormat();
        var extension = _imageExportService.GetExtension(format);
        var savedCount = 0;

        try
        {
            foreach (var image in checkedImages)
            {
                var fileName = BuildDefaultFileName(image);
                var destinationPath = GetAvailableDestinationPath(dialog.FolderName, fileName, extension);
                _imageExportService.Export(image.FilePath, destinationPath, format);
                savedCount++;
            }

            StatusText = $"已保存 {savedCount} 张图片到：{dialog.FolderName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText = $"已保存 {savedCount} 张图片，随后保存失败。";
        }
    }

    private void RefreshImages()
    {
        StatusText = "正在扫描 Windows 聚焦缓存...";
        _allImages.Clear();
        Images.Clear();
        PreviewImage = null;
        SelectedImage = null;

        var directories = _spotlightImageService.FindAssetDirectories();
        var desktopDirectoryCount = directories.Count(directory => directory.Source == SpotlightSource.Desktop);
        var lockScreenDirectoryCount = directories.Count(directory => directory.Source == SpotlightSource.LockScreen);

        ScanSummary = directories.Count == 0
            ? "未找到 Windows 聚焦缓存目录。"
            : $"扫描来源：桌面聚焦 {desktopDirectoryCount} 个目录，锁屏聚焦 {lockScreenDirectoryCount} 个目录。";

        _allImages.AddRange(_spotlightImageService.ScanImages(directories));
        ApplyImageFilter();

        StatusText = _allImages.Count == 0
            ? "没有找到可识别的壁纸图片。"
            : "扫描完成。";
    }

    private void ApplyImageFilter()
    {
        if (!IsLoaded && _allImages.Count == 0)
        {
            return;
        }

        var previousPath = SelectedImage?.FilePath;
        Images.Clear();

        foreach (var image in _allImages.Where(MatchesSelectedAspectFilter))
        {
            Images.Add(image);
        }

        SelectedImage = Images.FirstOrDefault(image => image.FilePath == previousPath) ?? Images.FirstOrDefault();
        UpdateImageCountText();
        StatusText = Images.Count == 0 && _allImages.Count > 0
            ? "当前比例筛选下没有图片。"
            : StatusText;
    }

    private bool MatchesSelectedAspectFilter(SpotlightImage image)
    {
        return SelectedAspectFilter switch
        {
            PortraitFilter => image.PixelHeight >= image.PixelWidth * 1.2,
            SquareLikeFilter => image.PixelWidth < image.PixelHeight * 1.2 && image.PixelHeight < image.PixelWidth * 1.2,
            _ => image.PixelWidth >= image.PixelHeight * 1.2
        };
    }

    private void UpdateImageCountText()
    {
        var checkedCount = _allImages.Count(image => image.IsChecked);
        ImageCountText = $"{Images.Count} / {_allImages.Count} 张图片，已勾选 {checkedCount} 张";
    }

    private void LoadSelectedPreview()
    {
        if (SelectedImage is null)
        {
            PreviewImage = null;
            return;
        }

        try
        {
            PreviewImage = _spotlightImageService.LoadPreview(SelectedImage.FilePath);
        }
        catch (Exception ex)
        {
            PreviewImage = null;
            StatusText = $"预览失败：{ex.Message}";
        }
    }

    private ExportFormat GetSelectedExportFormat()
    {
        return SelectedExportFormat switch
        {
            "PNG" => ExportFormat.Png,
            "AVIF" => ExportFormat.Avif,
            _ => ExportFormat.Jpeg
        };
    }

    private static string BuildDefaultFileName(SpotlightImage image)
    {
        var baseName = Path.GetFileNameWithoutExtension(image.FileName);
        var prefix = image.Source == SpotlightSource.Desktop ? "desktop-spotlight" : "lockscreen-spotlight";
        return string.IsNullOrWhiteSpace(baseName)
            ? prefix
            : $"{prefix}-{baseName}";
    }

    private static string GetAvailableDestinationPath(string folderPath, string fileNameWithoutExtension, string extension)
    {
        var safeFileName = string.Join("_", fileNameWithoutExtension.Split(Path.GetInvalidFileNameChars()));
        var destinationPath = Path.Combine(folderPath, safeFileName + extension);
        var index = 2;

        while (File.Exists(destinationPath))
        {
            destinationPath = Path.Combine(folderPath, $"{safeFileName}-{index}{extension}");
            index++;
        }

        return destinationPath;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
