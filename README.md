# Vantira V5

Vantira V5 is a polished Windows optimization suite built with **WinForms** on **.NET 8**.
It focuses on practical utilities, a professional dark UI, and safe optimization workflows.

## Key Features

- Modern dark interface with rounded corners and material-style controls
- Dynamic sidebar navigation with dedicated modules
- Async background actions for responsive UX
- Real diagnostics for performance and network visibility
- Safe cleanup workflows for temp files, logs, and recycle bin
- Hidden Easter eggs (optional, harmless, and user-triggered)

## Project Structure

```text
VantiraV5/
  Forms/
    MainForm.cs
    Controls/
      MaterialButton.cs
      SidebarNavButton.cs
    Sections/
      ISectionView.cs
      SectionControlBase.cs
      PerformanceSectionControl.cs
      GamingSectionControl.cs
      NetworkSectionControl.cs
      CleanupSectionControl.cs
      AboutSectionControl.cs
  Services/
    SystemOptimizationService.cs
  Utils/
    ThemePalette.cs
    UiEffects.cs
  Models/
    StartupItem.cs
    CleanupResult.cs
    NetworkStats.cs
    SystemSnapshot.cs
  Program.cs
  ApplicationConfiguration.cs
  VantiraV5.csproj
```

## Modules

### Performance
- CPU + RAM live snapshot
- Memory optimization simulation
- Startup app discovery and disable flow
- AI-style health summary generation

### Network
- Throughput estimate (download/upload)
- Ping/latency measurement
- Network stack repair commands (`flushdns`, `winsock reset`, `ip reset`)

### Cleanup
- Temp file cleanup with detailed results
- Recycle Bin cleanup
- Temporary log cleanup (`.log`, `.etl`, `.tmp`)

### Gaming
- High/ultimate performance power profile activation
- Safe FPS booster simulation mode

### About
- Product summary and module overview

## Easter Eggs (Chaos Mode)
Trigger by clicking specific optimization buttons **3+ times**.

Effects:
- A mini desktop pet that bounces around the app window
- Retro-style system event popup notifications
- A short “instant optimization” visual flash

All effects are cosmetic and non-destructive.

## Build & Run

> Requires Windows + .NET 8 SDK.

```bash
dotnet build VantiraV5.sln
dotnet run --project VantiraV5/VantiraV5.csproj
```

## Release Notes

- Designed for safe operation with defensive error handling
- Some actions may require elevated permissions depending on endpoint policies
- Recommended to code-sign binaries before public distribution
