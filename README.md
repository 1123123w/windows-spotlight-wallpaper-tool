# Windows Spotlight Wallpaper Tool - Save Desktop and Lock Screen Spotlight Images


Windows Spotlight Wallpaper Tool is a Windows 11 WPF app for browsing, selecting, and saving Windows Spotlight desktop wallpaper and lock screen wallpaper images. It helps you export Spotlight images as JPEG, PNG, or AVIF when AVIF export support is available.

功能说明
这是一款专为 Windows 11 设计的桌面聚焦（Windows Spotlight）图片提取工具，能够帮助用户轻松找到系统隐藏的聚焦壁纸位置，并一键导出高清原图。
网上关于“Windows Spotlight 壁纸在哪里”“如何保存聚焦图片”的问题非常多，本工具提供一站式自动化解决方案，无需手动查找复杂路径、无需第三方脚本，直接运行 exe 即可使用。
核心功能

自动读取聚焦图片目录
程序启动后自动定位 Windows 11 系统存储桌面聚焦图片的隐藏目录（%LOCALAPPDATA%\Packages\MicrosoftWindows.Client.CBS_cw5n1h2txyewy\...），无需用户手动查找路径，真正做到开箱即用。
图片列表预览
以缩略图形式展示所有可用的聚焦图片，支持快速浏览。清晰显示每张图片的基本信息（文件名、分辨率、文件大小等）。
大图浏览模式
点击任意缩略图即可进入大图查看模式，支持鼠标滚轮缩放、拖拽移动、全屏浏览，方便用户仔细查看图片细节和质量。
横屏 / 竖屏 智能筛选
提供比例筛选功能，一键切换显示横屏（16:9 等） 或 竖屏（9:16 等） 图片，特别适合手机壁纸爱好者快速挑选竖版高清图。
灵活导出保存
支持单张 / 批量导出
可选择保存格式：JPEG 或 PNG（PNG 保留透明信息，JPEG 体积更小）
自定义保存路径
自动重命名（可选添加日期、序号等）


使用特点

绿色免安装：单个 exe 文件，无需安装即可运行
界面简洁直观：采用现代化 UI，操作简单，上手快
高效快速：自动扫描 + 缓存机制，打开秒出图片列表
安全可靠：仅读取系统壁纸目录，不修改任何系统文件

适用人群

喜欢 Windows 聚焦壁纸但不知道怎么保存的用户
需要大量高清横版/竖版壁纸做手机/电脑桌面的人
壁纸收集爱好者
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
