# Changelog

All notable changes to Roost are documented here.
This project follows [Semantic Versioning](https://semver.org/): every change,
addition or fix bumps the version number.

## [1.1.0] - 2025-11-25

Added Linux app support.

### Added
- **Linux apps (WSL)** page — run Linux GUI software on Windows 11 through
  **WSL 2 + WSLg**:
  - Detects WSL and lists installed distributions; clear guidance when WSL is
    missing (`wsl --install`).
  - Installs packages with `apt` from inside the app.
  - Enumerates installed GUI apps from their `.desktop` entries and **launches**
    them (WSLg renders the window), or pins a **native Start-Menu shortcut** that
    re-launches them through Roost (`--launch-linux`, no console flash).
- English + Hebrew strings for the Linux page.

### Notes
- Like Android, Roost orchestrates the runtime (here WSL) rather than replacing
  it; WSL 2 requires the Virtual Machine Platform feature.

## [1.0.0] - 2025-11-25

First release — Android support.

### Added
- Install Android apps on Windows 11 by driving an Android backend over ADB,
  backend-agnostic by design:
  - A community **Windows Subsystem for Android** build (apps feel native), or
  - Any emulator that exposes ADB (**BlueStacks, LDPlayer, MuMu, Nox**, …).
- **Automatic engine detection** — probes the known ADB endpoints and lists
  whatever is connected; add a custom `host:port` too.
- **Install** `.apk` and split bundles (`.xapk` / `.apks` / `.apkm`) via
  `adb install` / `install-multiple`, with runtime permissions granted.
- **Metadata & icon** parsed with aapt2 when present (optional — install works
  without it).
- **My apps** — list third-party packages per engine; **launch**, **uninstall**,
  and create a **native Start-Menu / Desktop shortcut** that re-launches the app
  through Roost with its own icon.
- **Readiness check** — reports hypervisor, CPU virtualization and the Virtual
  Machine Platform feature, with guidance (and a clear note that a managed work
  PC may block these).
- **One-click adb download** (Google platform-tools) when adb is missing.
- **Professional Fluent UI**, light / dark / system theme, **English + Hebrew**
  (full RTL).
- Settings persisted to `%APPDATA%\Roost\settings.json`; optional start with
  Windows; "suspend engine when idle" preference.
- Command-line: `--install <file>` (open on the Install page) and
  `--launch <engineId> <package>` (used by app shortcuts, headless).

### Known limitations
- An Android **runtime engine and hardware virtualization are required** — Roost
  orchestrates them, it does not emulate Android itself. On a locked corporate
  machine these may be blocked by IT policy.
- Microsoft discontinued the official WSA on 2025-03-05; the native-feel path now
  relies on a community WSA build.

### Planned
- iOS / macOS app support is on the roadmap and will follow the same quality bar
  once a viable runtime path is validated. (It is **not** functional today — see
  the README for why this is hard.)
