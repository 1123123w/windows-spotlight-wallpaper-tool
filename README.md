# Windows Spotlight Wallpaper Tool - Save Desktop and Lock Screen Spotlight Images

Windows Spotlight Wallpaper Tool is a Windows 11 WPF app for browsing, selecting, and saving Windows Spotlight desktop wallpaper and lock screen wallpaper images. It helps you export Spotlight images as JPEG, PNG, or AVIF when AVIF export support is available.

中文说明：这个工具用于查找和保存 Windows 聚焦壁纸，包括 Windows 11 桌面聚焦、锁屏聚焦、壁纸保存、壁纸导出，以及 JPEG、PNG、AVIF 另存格式。

Repository: <https://github.com/1123123w/windows-spotlight-wallpaper-tool>  
Author: [1123123w](https://github.com/1123123w)  
License: GPL-3.0-or-later

## Features

- Browse Windows Spotlight desktop wallpaper images from the current user profile.
- Browse Windows Spotlight lock screen images stored without normal file extensions.
- Filter images by aspect ratio: wide, portrait, or square-like.
- Select one or more images with checkboxes.
- Save selected Spotlight wallpapers as JPEG, PNG, or AVIF.
- Keep the original cache files untouched.

## Why This Tool Exists

Windows Spotlight downloads high-quality wallpaper images, but the files are stored in hidden application cache folders. Some Spotlight files have no extension, and desktop Spotlight and lock screen Spotlight can use different storage locations. This tool scans the known Windows Spotlight locations, validates real image files, shows previews, and exports the selected wallpapers to a normal folder.

## Supported Windows Spotlight Locations

The app scans both common Windows Spotlight sources:

```text
%LOCALAPPDATA%\Packages\Microsoft.Windows.ContentDeliveryManager_*\LocalState\Assets
%LOCALAPPDATA%\Packages\MicrosoftWindows.Client.CBS_*\LocalCache\Microsoft\IrisService\*
```

`ContentDeliveryManager` is commonly used by lock screen Spotlight. `MicrosoftWindows.Client.CBS` / `IrisService` is used by desktop Spotlight on current Windows 11 builds.

## Export Formats

- JPEG uses WPF `JpegBitmapEncoder`.
- PNG uses WPF `PngBitmapEncoder`.
- AVIF export is enabled only when the `magick` command from ImageMagick is available in `PATH`.

Windows 11 can often view AVIF files, but viewing AVIF does not guarantee that WPF can encode AVIF directly. This app keeps JPEG and PNG export native and stable, and uses ImageMagick for AVIF export when available.

## Requirements

- Windows 11 or modern Windows 10
- .NET 8 SDK for building from source
- Optional: ImageMagick for AVIF export

## Build and Run

```powershell
dotnet build
dotnet run
```

To run the debug build directly:

```powershell
.\bin\Debug\net8.0-windows\WindowsSpotlightWallpaperTool.exe
```

## FAQ

### Where are Windows Spotlight desktop wallpapers stored?

On current Windows 11 builds, desktop Spotlight wallpapers can be cached under:

```text
%LOCALAPPDATA%\Packages\MicrosoftWindows.Client.CBS_*\LocalCache\Microsoft\IrisService\*
```

The exact folder name can change by user profile and Windows build, so this app scans matching package folders automatically.

### How do I save Windows Spotlight lock screen images?

Lock screen Spotlight images are commonly stored under:

```text
%LOCALAPPDATA%\Packages\Microsoft.Windows.ContentDeliveryManager_*\LocalState\Assets
```

Many files in this folder have no extension. This app decodes them as images, filters invalid cache files, and lets you save the selected images as normal JPEG, PNG, or AVIF files.

### Can Windows 11 export AVIF?

Windows 11 can often decode and display AVIF, but AVIF encoding support is not exposed through a simple WPF encoder like JPEG and PNG. This app enables AVIF export when ImageMagick's `magick` command is available.

### Why are Spotlight files missing extensions?

Windows Spotlight stores many images as application cache files rather than user-facing image files. The content can still be a valid image even when the file has no `.jpg`, `.png`, or `.avif` extension.

## Suggested GitHub Topics

`windows-spotlight`, `windows-11`, `wallpaper`, `desktop-wallpaper`, `lock-screen`, `wpf`, `csharp`, `jpeg`, `png`, `avif`
