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

// TODO: Add your initialization logic here
_consoleHandler = new ConsoleHandler(new ConsoleLogger(), new ConsoleCommander());
_consoleHandler.AttachToWindow(Window);


int startLevel = 0;
int width = 10;
int height = 10;
bool isDebugMode = true;
// SETUP THE MONCON CONSOLE COMMANDER//
//You can setup the console commander by creating a node, then attatch new nodes to it.

//Create a node for Game specific commands
var gameNode = new ConsoleCommandNode("game");
//Create a node for grid specific commands
var gridNode = new ConsoleCommandNode("grid");
//Create a node for debug commands
var debugNode = new ConsoleCommandNode("debug");


var loadLevelNode = new AnyNumbersParameterNode("loadLevel", startLevel.ToString(), LoadLevel);
//Create a parameter node for setting the grid size
var gridSizeNode = new NumbersPairParameterNode("gridSize", (width, height), (x, y) => { SetGridSize(x, y); });
//Create a parameter node for exiting the game
var exitNode = new MethodParameterNode("exit", new ConsoleCommandSignature("now", () => { System.Environment.Exit(0); }));
//Create a parameter node for toggling debug mode
var debugModeNode = new ToggleParameterNode("debugMode", isDebugMode ? "on" : "off", SetDebugModeOn, SetDebugModeOff);


gameNode.AddNode(exitNode);
gameNode.AddNode(loadLevelNode);

gridNode.AddNode(gridSizeNode);
            
debugNode.AddNode(debugModeNode);

_consoleHandler.AddNode(gameNode);
_consoleHandler.AddNode(gridSizeNode);
_consoleHandler.AddNode(debugNode);

// In your Game class Update method
consoleHandler.Update(gameTime);

// In your Game class Draw method
consoleHandler.Draw(spriteBatch);
```


## USAGE
- Use the _consoleHandler.ToggleConsole() to open/close the console
- Use ":" to start a command - pressing enter will list the available nodes in the current node
- Use "->" to invoke a command 

Example:
1. Open Console
2. :game:gridsize->12,12 

## Requirements

- .NET 8.0
- MonoGame Framework 3.8.1.303

## Installation

Install via NuGet Package Manager or use the Package Manager Console:

```
Install-Package MonConLib
```
