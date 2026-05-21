using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace WindowsSpotlightWallpaperTool;

public enum SpotlightSource
{
    LockScreen,
    Desktop
}

public sealed class SpotlightImage : INotifyPropertyChanged
{
    private bool _isChecked;

    public event PropertyChangedEventHandler? PropertyChanged;

    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public required long FileSizeBytes { get; init; }
    public required int PixelWidth { get; init; }
    public required int PixelHeight { get; init; }
    public required SpotlightSource Source { get; init; }
    public required BitmapSource Thumbnail { get; init; }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        }
    }

    public string Dimensions => $"{PixelWidth} x {PixelHeight}";
    public string FileSize => FormatFileSize(FileSizeBytes);
    public string SourceLabel => Source == SpotlightSource.Desktop ? "桌面聚焦" : "锁屏聚焦";

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        var kib = bytes / 1024d;
        if (kib < 1024)
        {
            return $"{kib:0.#} KB";
        }

        return $"{kib / 1024d:0.##} MB";
    }
}
