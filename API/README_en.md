# IPAndSiteBlockerAPI

A .NET library for blocking messages containing URLs, IP addresses, and domains, with whitelist support. Designed for use in CounterStrikeSharp plugins.

## Features

- Detects and blocks messages with URLs, IPs, and domains
- Supports whitelisting of allowed sites/addresses
- Cleans player names by removing blocked content
- Compatible with dependency injection in CounterStrikeSharp plugins

## Installation

Add the IPAndSiteBlockerAPI.dll to your project references or use NuGet if available.

## Usage

### Basic Usage

```csharp
using IPAndSiteBlockerAPI;

// Create instance with whitelist
var whitelist = new List<string> { "google.com", "192.168.1.1" };
var blockChecker = new BlockChecker(whitelist);

// Check if message is blocked
bool isBlocked = blockChecker.IsBlocked("Check out http://example.com");

// Clean player name
string cleanName = blockChecker.CleanName("Player with http://bad.com");
```

### In CounterStrikeSharp Plugin

Register the service in your plugin's `Load` method:

```csharp
public override void Load(bool hotReload)
{
    // Register BlockChecker as singleton
    Services.RegisterSingleton<IBlockChecker>(() => new BlockChecker(new List<string> { "allowed.com" }));
}
```

Then inject it into your classes:

```csharp
public class MyPlugin : BasePlugin
{
    private readonly IBlockChecker _blockChecker;

    public MyPlugin(IBlockChecker blockChecker)
    {
        _blockChecker = blockChecker;
    }

    // Use _blockChecker in your methods
}
```

## API Reference

### IBlockChecker Interface

- `List<string> Whitelist { get; set; }` - List of whitelisted items
- `bool IsBlocked(string message)` - Returns true if message contains blocked content
- `string CleanName(string name)` - Removes blocked content from name
- `bool IsWhitelisted(string message)` - Checks if message is whitelisted

### BlockChecker Class

Implements `IBlockChecker`. Constructor accepts optional `IEnumerable<string>` for initial whitelist.

## Notes

- Regex patterns are pre-compiled for performance
- Checks for common domain extensions
- Case-insensitive matching