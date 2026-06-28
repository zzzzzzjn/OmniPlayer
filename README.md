<p align="center">
  <img src="src/OmniphonyLauncher/assets/omniplayer.png" width="180" alt="OmniPlayer icon">
</p>

<h1 align="center">OmniPlayer</h1>

<p align="center">
  A Windows integration launcher for Omniphony Studio, mpv-omniphony and compatible renderer bridge components.
</p>

## 中文介绍

OmniPlayer 是一个面向 Windows 的一键集成、配置与启动程序。它把 **Omniphony Studio**、**mpv-omniphony**、`orender` 和兼容 bridge 的路径发现、配置管理、OSC 监看及音频设备选择集中到一个可视化界面中。

<img width="1098" height="783" alt="屏幕截图 2026-06-28 180116" src="https://github.com/user-attachments/assets/6eba53e6-d8e9-47bc-bb22-54eafd5532f1" />

OmniPlayer **不是新的 Dolby 解码器，也不自行实现、修改或逆向任何专有编解码算法**。媒体解析、空间渲染及 bridge 能力均来自各自独立开发、独立授权的上游开源组件；OmniPlayer 的职责只是安装编排、配置保护和进程启动。项目与 Dolby Laboratories、Audinate 及相关商标权利人不存在隶属、背书或合作关系。
## Features

- Unified black-and-purple Windows UI
- Portable relative-runtime discovery with installed-runtime fallback
- One-click launch of mpv-omniphony and Omniphony Studio
- Safe `config.yaml` bridge-path updates with timestamped backups
- OSC supervision with the mpv overlay disabled by default
- ASIO/WASAPI device discovery, including Dante Virtual Soundcard when installed
- Drag-and-drop media selection and explicit argument handling without `cmd.exe`
- Diagnostics for runtime files, `ad_orender`, bridge configuration and Dante ASIO visibility

## Architecture

```text
OmniPlayer
  ├─ starts mpv-omniphony with explicit renderer/config arguments
  ├─ starts Omniphony Studio as the OSC supervision UI
  ├─ selects the requested Windows audio endpoint (ASIO/WASAPI)
  └─ preserves and updates the shared Omniphony YAML configuration
```

The upstream applications remain separate, supervised processes. This avoids fragile window embedding and preserves their native playback, renderer and visualization implementations while presenting one entry point to the user.

## Credits and thanks

OmniPlayer exists because of the work of these projects and their contributors:

- [mgth/Omniphony](https://github.com/mgth/Omniphony) — `liborender`, `orender` and Omniphony Studio
- [mgth/mpv-omniphony](https://github.com/mgth/mpv-omniphony) — mpv renderer integration, OSC overlay support and Windows ASIO output
- [harletty/harletty-bridge](https://github.com/harletty/harletty-bridge) — compatible decoder bridge plugin
- [mpv-player/mpv](https://github.com/mpv-player/mpv) — the media player foundation
- [truehdd/truehdd](https://github.com/truehdd/truehdd) and the decoder contributors credited by harletty-bridge

Thank you to every upstream maintainer and contributor. OmniPlayer does not claim authorship of their playback, rendering, bridge or codec work.

## Build from source

Requirements: Windows and .NET 8 SDK.

```powershell
dotnet build src/OmniphonyLauncher/OmniphonyLauncher.csproj -c Release
dotnet run --project tests/OmniphonyLauncher.SmokeTests/OmniphonyLauncher.SmokeTests.csproj -c Release
```

To build a local portable aggregate from already installed/downloaded upstream runtimes:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/build-portable.ps1 -Version 1.3
```

The packaging script does not download or circumvent licensing for proprietary software. Dante Virtual Soundcard is never bundled; it is detected as an external system driver.

## Releases and licensing

The OmniPlayer launcher source is MIT licensed. Portable release archives are aggregate distributions: every bundled upstream binary retains its own license. Keep the archive's `legal/` directory intact when redistributing it. Exact component versions, corresponding-source links and notices are documented in [legal/SOURCE_AND_LICENSES.md](legal/SOURCE_AND_LICENSES.md).

Names such as Dolby, Dolby Atmos, Dante and Audinate may be trademarks of their respective owners and are used only for factual compatibility descriptions.
