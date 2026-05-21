using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace WindowsSpotlightWallpaperTool;

public enum ExportFormat
{
    Jpeg,
    Png,
    Avif
}

public sealed class ImageExportService
{
    private readonly string? _magickPath;

    public ImageExportService()
    {
        _magickPath = FindExecutable("magick.exe") ?? FindExecutable("magick");
    }

    public bool IsAvifSupported => _magickPath is not null;

    public string AvifSupportDescription => IsAvifSupported
        ? "AVIF export is available through ImageMagick."
        : "AVIF export is unavailable. Install ImageMagick and make magick available in PATH to enable it.";

    public string GetExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Jpeg => ".jpg",
            ExportFormat.Png => ".png",
            ExportFormat.Avif => ".avif",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format.")
        };
    }

    public void Export(string sourcePath, string destinationPath, ExportFormat format)
    {
        switch (format)
        {
            case ExportFormat.Jpeg:
                EncodeWithWpf(sourcePath, destinationPath, new JpegBitmapEncoder { QualityLevel = 95 });
                break;
            case ExportFormat.Png:
                EncodeWithWpf(sourcePath, destinationPath, new PngBitmapEncoder());
                break;
            case ExportFormat.Avif:
                ExportAvif(sourcePath, destinationPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format.");
        }
    }

    private static void EncodeWithWpf(string sourcePath, string destinationPath, BitmapEncoder encoder)
    {
        using var sourceStream = File.OpenRead(sourcePath);
        var decoder = BitmapDecoder.Create(
            sourceStream,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);

        encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0]));

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        using var outputStream = File.Create(destinationPath);
        encoder.Save(outputStream);
    }

    private void ExportAvif(string sourcePath, string destinationPath)
    {
        if (_magickPath is null)
        {
            throw new NotSupportedException(AvifSupportDescription);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        var startInfo = new ProcessStartInfo
        {
            FileName = _magickPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        startInfo.ArgumentList.Add(sourcePath);
        startInfo.ArgumentList.Add(destinationPath);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Unable to start ImageMagick.");
        var error = process.StandardError.ReadToEnd();
        process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? "ImageMagick failed to export AVIF."
                : error.Trim());
        }
    }

    private static string? FindExecutable(string fileName)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(directory.Trim(), fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
                // Ignore malformed PATH entries.
            }
        }

        return null;
    }
}
