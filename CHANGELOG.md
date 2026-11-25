# Changelog

All notable changes to Roost are documented here.
This project follows [Semantic Versioning](https://semver.org/): every change,
addition or fix bumps the version number.

## [1.2.1] - 2025-11-25

### Fixed (QA pass)
- Installed-app and Linux-app lists no longer duplicate entries when reloads
  overlap (reload-token guard).
- **Uninstall now asks for confirmation** before removing an app and its data.
- Install success is detected from combined stdout+stderr ("Success" without
  "Failure"), so a successful install on stderr-emitting adb builds isn't
  misreported as a failure.
- The Install button re-enables correctly when you select an engine after
  loading a file; guard against an empty split bundle.
- WSL package install uses `sudo -n` so it fails fast instead of hanging when the
  distro needs a sudo password.
- ProcessRunner waits asynchronously and honours cancellation (no blocked
  thread-pool thread for the whole timeout).
- Linux shortcut arguments are quote-escaped; dead code removed.
- Leftover split-bundle temp extractions are swept on startup; custom ADB port
  is range-validated; COM shortcut objects are released; the chosen default
  engine is persisted immediately; a theme-only change no longer rebuilds the
  window.

## [1.2.0] - 2025-11-25

Added an in-app user guide.

### Added
- **Help / Guide** page in the sidebar — a full, bilingual (English + Hebrew)
  user guide built into the app:
  - Top-tab navigation across sections: Getting started, Set up an engine,
    Install an app, Manage apps, Linux apps, Settings, Troubleshooting.
  - Each section shows an **annotated screenshot** with numbered callouts that
    match numbered step-by-step explanations.
  - Screenshots and callouts mirror correctly for Hebrew (RTL).

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
