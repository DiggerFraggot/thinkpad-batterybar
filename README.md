# thinkpad-batterybar

A taskbar battery widget for Windows 11, built as a replacement for the battery gauge that used to be available in **Lenovo Vantage** — removed in newer versions of the app and unavailable on Windows 11.

Displays a live battery bar directly on the taskbar, next to the system clock.

![preview](preview.png)

---

## Features

- Battery percentage or remaining time displayed on the taskbar
- Color changes based on charge level (green → yellow → red)
- Charging indicator (plug icon) when connected to power
- Power plan switcher (High performance / Balanced / Power saver) via click menu
- Auto-hides when any app goes fullscreen (games, YouTube, etc.)
- Stays on top of the taskbar at all times
- Language support: Polish / English (saved to registry)
- Tray icon with current battery level

---

## Bugs

- can sometimes hide behind taskbar - i need to work on that but for now i use translucentTB

---

## Requirements

- Windows 11
- .NET 8 SDK - [download](https://dotnet.microsoft.com/download/dotnet/8)

---

## Build

```
dotnet publish -c Release
```

Output: `bin\Release\net8.0-windows\win-x64\publish\BatteryBar.exe`

---

## Usage

Run `BatteryBar.exe`. The widget appears on the taskbar to the left of the clock.

- **Left click** the widget → power plan menu
- **Right click tray icon** → show/hide, switch language, quit

To run on startup, add a shortcut to `BatteryBar.exe` in:
```
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
```

---

## Configuration

Edit the `CFG` class at the top of `Program.cs` before building:

| Setting | Description |
|---|---|
| `PILL_W` / `PILL_H` | Battery bar size in pixels |
| `FONT_FAMILY` / `FONT_SIZE` | Label font |
| `OFFSET_FROM_TRAY` | Distance from the clock area |
| `COLOR_HIGH/MED/LOW/CRIT` | Colors per charge level |
| `THRESH_HIGH/MED/LOW` | Percentage thresholds for color changes |
| `LANG_DEFAULT` | Default language (`"pl"` or `"en"`) |
| `UPDATE_MS` | How often battery status is refreshed (ms) |

---

## Why

Lenovo Vantage used to show a small battery gauge on the taskbar. That feature was quietly removed. This project brings it back as a standalone app that works on any Windows 11 machine — ThinkPad or otherwise.

---

## License

MIT
