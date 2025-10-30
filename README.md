# MonConLib

A console library for MonoGame Framework applications that provides an in-game debug console with command tree navigation.

## Features

- **Interactive Console**: Toggle-able in-game console overlay
- **Command Tree Structure**: Hierarchical command organization
- **Parameter Support**: Various parameter types (toggle, numbers, strings, methods)
- **Logging System**: Built-in logging with timestamps
- **Keyboard Input**: Full keyboard input handling with history scrolling

## Usage

```csharp
// Create logger and commander
var logger = new ConsoleLogger();
var commander = new ConsoleCommander();

// Create console handler
var consoleHandler = new ConsoleHandler(logger, commander);

// Add commands
commander.AddNode(new ToggleParameterNode("on", "off", () => EnableDebug(), () => DisableDebug()));

// In your Game class Update method
consoleHandler.Update(gameTime);

// In your Game class Draw method
consoleHandler.Draw(spriteBatch);
```

## Requirements

- .NET 8.0
- MonoGame Framework 3.8.1.303

## Installation

Install via NuGet Package Manager or use the Package Manager Console:

```
Install-Package MonConLib
```
