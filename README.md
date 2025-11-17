# OutletTweaks

A BepInEx mod for Returns Outlet Simulator that increases game difficulty through configurable tweaks.

## Features

- **Customer Multiplier**: Spawn 2x (or more) customers per spawn event
- **Market Price Reduction**: Buy products at 80% of original price (configurable)
- **Rent Increase**: Increases shop rent by 20x (configurable)

All values are fully configurable via config file - no recompilation needed.

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file into your game directory
3. Start the game - the mod will load automatically

For detailed instructions, see `Installationsanleitung.txt` inside the release ZIP (English + German).

## Configuration

After first launch, edit the config file at:
```
BepInEx/config/bb.outlettweaks.cfg
```

Example configuration:
```ini
[Kunden]
Multiplikator = 2.0          # 2x customers (change to 3.0 for 3x)

[Marktpreise]
Prozent = 0.8                # 80% price (0.5 = 50% discount)

[Miete]
Multiplikator = 20.0         # 20x rent (change to 10.0 for 10x)
```

Restart the game after changing values.

## System Requirements

- Returns Outlet Simulator (Steam)
- Windows 10/11 (64-bit)
- 8 GB RAM recommended (16 GB for best performance)

Note: This mod increases system load due to more active customers. Performance may vary on lower-end systems.

## Technical Details

- Built with BepInEx 6.0 (IL2CPP)
- Uses Harmony patches for runtime code modification
- Reflection-based access to game classes
- Delayed patching to prevent startup crashes

## Development

### Building from Source

Requirements:
- .NET 6.0 SDK
- Game installation with BepInEx

1. Clone the repository
2. Create `OutletTweaks.csproj.user` with your game path:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <OUTLET_GAME_DIR>C:\Path\To\Your\Game</OUTLET_GAME_DIR>
  </PropertyGroup>
</Project>
```
3. Build: `dotnet build -c Release`
4. DLL will be in `bin/Release/net6.0/OutletTweaks.dll`

### Project Structure

- `Plugin.cs` - Main plugin entry point, config handling, delayed patching
- `CustomerSpawnTweaks.cs` - Customer spawn multiplier with recursion prevention
- `MarketPriceTweaks.cs` - Market price modification (patches both UI and backend)
- `BillTweaks.cs` - Rent multiplier (patches multiple bill calculation methods)

## Known Issues

- HarmonyX warnings in log are normal and harmless (IL2CPP compiler artifacts)
- On very low-end systems, game may crash when loading savegames (wait 10 seconds after game start before loading)

## Version History

**v1.7.2** (November 2025)
- Initial public release
- All features functional and tested
- Fully configurable via config file
- Delayed patching prevents startup crashes

## Limitations / Testing

This is a small learning project as part of my training as an application developer.
There are currently no automated tests for this mod – I test changes manually in the game
(new save, check rent, prices and number of customers).
In future projects I plan to add unit tests for important calculations.

## License

Free to use and modify. No commercial use.

## Credits

Created for the Returns Outlet Simulator community.

Uses:
- [BepInEx](https://github.com/BepInEx/BepInEx) - Modding framework
- HarmonyX - Runtime patching
