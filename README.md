# Vantira V5 (Modernized)

Vantira V5 is a modern Windows optimizer-style desktop application built with **WinForms** on **.NET 8**.

## Highlights

- Sleek dark theme with rounded corners and material-style buttons
- Sidebar navigation with dynamically loaded sections:
  - Performance
  - Gaming
  - Network
  - Cleanup
  - About
- Smooth UX touches: hover states, status updates, subtle transitions, tooltips
- Chaotic Easter eggs triggered by repeated button clicks

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

## Functional Modules

### Performance
- Reads CPU and RAM usage
- Performs memory optimization simulation via GC compaction cycle
- Quick temp cleanup
- Startup apps listing + disable selected entries

### Network
- Measures estimated upload/download throughput
- Ping test to `8.8.8.8`
- Runs network repair commands (`flushdns`, `winsock reset`, `int ip reset`)

### Cleanup
- Clears temp files
- Empties recycle bin
- Clears common temporary logs (`.log`, `.etl`, `.tmp`)

### Gaming
- Enables high-performance (or ultimate-performance) power profile
- Includes harmless FPS booster simulation

## Easter Eggs (Chaos Mode)
Click the same optimization action button **3+ times** in these sections:
- Performance optimize memory
- Network optimize
- Cleanup temp cleanup
- Gaming power mode

What happens:
- Mini desktop pet appears and bounces around the app
- Random Minecraft-style warning popup appears
- Instant visual “optimization flash” effect

All Easter eggs are harmless and do not apply destructive system changes.

## Build and Run

1. Install .NET 8 SDK on Windows.
2. Build:
   ```bash
   dotnet build VantiraV5.sln
   ```
3. Run:
   ```bash
   dotnet run --project VantiraV5/VantiraV5.csproj
   ```

## Notes
- Some operations may require elevated privileges depending on system policy.
- This project is designed as an optimizer utility demo with safe defaults.
