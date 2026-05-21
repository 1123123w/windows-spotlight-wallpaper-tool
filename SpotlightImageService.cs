using System.IO;
using System.Windows.Media.Imaging;

namespace WindowsSpotlightWallpaperTool;

public sealed class SpotlightAssetDirectory
{
    public required string Path { get; init; }
    public required SpotlightSource Source { get; init; }
}

public sealed class SpotlightImageService
{
    private const long MinimumImageBytes = 50 * 1024;
    private const int ThumbnailDecodeWidth = 220;

    public IReadOnlyList<SpotlightAssetDirectory> FindAssetDirectories()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            return Array.Empty<SpotlightAssetDirectory>();
        }

        var packagesPath = Path.Combine(localAppData, "Packages");
        if (!Directory.Exists(packagesPath))
        {
            return Array.Empty<SpotlightAssetDirectory>();
        }

        var directories = new List<SpotlightAssetDirectory>();
        directories.AddRange(FindLockScreenDirectories(packagesPath));
        directories.AddRange(FindDesktopDirectories(packagesPath));

        return directories
            .GroupBy(directory => directory.Path, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(directory => directory.Source)
            .ThenBy(directory => directory.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<SpotlightImage> ScanImages(IEnumerable<SpotlightAssetDirectory> assetDirectories)
    {
        var images = new List<SpotlightImage>();

        foreach (var directory in assetDirectories.DistinctBy(directory => directory.Path, StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(directory.Path))
            {
                continue;
            }

            foreach (var filePath in Directory.EnumerateFiles(directory.Path))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length < MinimumImageBytes)
                {
                    continue;
                }

                var image = TryCreateImage(fileInfo, directory.Source);
                if (image is not null)
                {
                    images.Add(image);
                }
            }
        }

        return images
            .OrderBy(image => image.Source)
            .ThenByDescending(image => image.PixelWidth * image.PixelHeight)
            .ThenByDescending(image => image.FileSizeBytes)
            .ThenBy(image => image.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public BitmapSource LoadPreview(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);

        var frame = decoder.Frames[0];
        frame.Freeze();
        return frame;
    }

    private static IEnumerable<SpotlightAssetDirectory> FindLockScreenDirectories(string packagesPath)
    {
        return Directory
            .EnumerateDirectories(packagesPath, "Microsoft.Windows.ContentDeliveryManager_*")
            .Select(path => Path.Combine(path, "LocalState", "Assets"))
            .Where(Directory.Exists)
            .Select(path => new SpotlightAssetDirectory
            {
                Path = path,
                Source = SpotlightSource.LockScreen
            });
    }

    private static IEnumerable<SpotlightAssetDirectory> FindDesktopDirectories(string packagesPath)
    {
        foreach (var packagePath in Directory.EnumerateDirectories(packagesPath, "MicrosoftWindows.Client.CBS_*"))
        {
            var irisServicePath = Path.Combine(packagePath, "LocalCache", "Microsoft", "IrisService");
            if (!Directory.Exists(irisServicePath))
            {
                continue;
            }

            foreach (var imageDirectory in Directory.EnumerateDirectories(irisServicePath))
            {
                yield return new SpotlightAssetDirectory
                {
                    Path = imageDirectory,
                    Source = SpotlightSource.Desktop
                };
            }
        }
    }

    private static SpotlightImage? TryCreateImage(FileInfo fileInfo, SpotlightSource source)
    {
        try
        {
            using var stream = fileInfo.OpenRead();
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var frame = decoder.Frames[0];
            if (frame.PixelWidth < 450 || frame.PixelHeight < 450)
            {
                return null;
            }

            var thumbnail = CreateThumbnail(fileInfo.FullName);
            thumbnail.Freeze();

            return new SpotlightImage
            {
                FilePath = fileInfo.FullName,
                FileName = fileInfo.Name,
                FileSizeBytes = fileInfo.Length,
                PixelWidth = frame.PixelWidth,
                PixelHeight = frame.PixelHeight,
                Source = source,
                Thumbnail = thumbnail
            };
        }
        catch
        {
            return null;
        }
    }

    private static BitmapSource CreateThumbnail(string filePath)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DecodePixelWidth = ThumbnailDecodeWidth;
        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
        bitmap.EndInit();
        return bitmap;
    }
}
