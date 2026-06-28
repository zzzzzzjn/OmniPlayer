# OmniPlayer v1.4.0

OmniPlayer v1.4.0 turns the Omniphony toolchain into a more approachable Windows spatial-audio workflow. It brings runtime discovery, shared-path configuration, mpv-omniphony playback, Omniphony Studio monitoring, OSC supervision, and ASIO/WASAPI device selection into one focused launcher.

OmniPlayer v1.4.0 将 Omniphony 工具链整理为更易使用的 Windows 空间音频工作流：统一完成运行组件发现、共享路径配置、mpv-omniphony 播放、Omniphony Studio 监看、OSC 会话管理以及 ASIO/WASAPI 输出设备选择。

## Highlights

- Redesigned black interface with restrained purple accents, rounded controls, and clearer information hierarchy
- Chinese and English interface switching
- Clickable product logo with a dedicated About dialog, version, developer, contact, copyright, and licensing information
- Portable relative-runtime discovery with installed-runtime fallback
- One-click supervised startup of mpv-omniphony and Omniphony Studio
- OSC object monitoring in Studio with the duplicate mpv overlay disabled by default
- ASIO/WASAPI endpoint discovery, including Dante Virtual Soundcard when installed
- AoIP-friendly output selection without bundling or replacing third-party audio drivers
- Safe `config.yaml` bridge-path updates with timestamped backups
- Diagnostics for runtime files, `ad_orender`, bridge configuration, and Dante ASIO visibility
- Multi-file self-contained .NET packaging for faster cold startup

## Scope and upstream components

OmniPlayer is an integration, configuration, and launch utility. It does not implement or claim authorship of proprietary codec technology. Decoding, playback, bridge, OSC, and rendering capabilities are supplied by independently developed and independently licensed upstream projects, including Omniphony, mpv-omniphony, harletty-bridge, mpv, and their credited dependencies.

The portable archive is an aggregate distribution. Every bundled upstream binary retains its own copyright and license. Keep the archive's `legal/` directory intact when redistributing it. Dante Virtual Soundcard is not bundled.

## Developers

Jacky Zhang · Eleanor Ye<br>
Contact: zjn4576@gmail.com

## Download verification

File: `OmniPlayer-Portable-v1.4.0.zip`<br>
SHA-256: `1AEE4E1AA32A3972A1E1318AA53D3199F92BA8EA3BDE5E0B8AFA3719E8CF8C2C`
