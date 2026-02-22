# Unity Advanced Logger

Enhanced logging system for Unity with in-game console, automatic class prefixing, and production handling.

## Features

### Core Features
- **Automatic prefix** with the calling class name
- **Production Mode** - Disables all logs except errors
- **In-Game Console** - Real-time log visualization
- **Filtering** by severity level
- **Search** within logs
- **Log history** with memory limit
- **Colors** based on severity level
- **Metadata** - Timestamp, class, method, line

### Log Levels

| Level | Color | Production | Description |
|--------|---------|------------|-------------|
| Debug | Gray | Disabled | Detailed debug logs |
| Info | White | Disabled | General information |
| Warning | Yellow | Disabled | Warnings |
| Error | Light Red | Active | Errors |
| Fatal | Red | Active | Critical errors |

## Installation

### 1. Copy the Files

```
UnityLogger/
├── Runtime/
│   ├── LogLevel.cs
│   ├── LogEntry.cs
│   ├── Log.cs
│   ├── InGameConsole.cs
│   └── Examples/
│       └── LoggingExample.cs
```

Place the `UnityLogger` folder inside your Unity project (usually `Assets/Scripts/`).

### 2. Add the In-Game Console

1. Create an empty GameObject in your scene
2. Rename it "InGameConsole"
3. Add the `InGameConsole` component
4. Configure the shortcut key (default: `~` or BackQuote)

### 3. Mark as DontDestroyOnLoad (Optional)

The console is already configured to persist between scenes. If you want it to be global, make sure it exists in your initialization scene.

## Usage

### Basic Logging

```csharp
using UnityLogger;

public class PlayerController : MonoBehaviour
{
    private void Start()
    {
        // Simple logs - the [PlayerController] prefix is added automatically
        Log.Debug("Player initialized");
        Log.Info("Player health: 100");
        Log.Warning("Low ammunition!");
        Log.Error("Failed to load player data");
        Log.Fatal("Critical game state error");
    }
}
```

**Result in the Unity console**:
```
[PlayerController] Player initialized
[PlayerController] Player health: 100
[PlayerController] Low ammunition!
[PlayerController] Failed to load player data
[PlayerController] Critical game state error
```

### Logging with Variables

```csharp
public class GameManager : MonoBehaviour
{
    private int score = 1000;
    private Vector3 position = new Vector3(10, 20, 30);

    private void Update()
    {
        Log.Info($"Current score: {score}");
        Log.Debug($"Player position: {position}");
    }
}
```

### Exception Logging

```csharp
public class DataLoader : MonoBehaviour
{
    private void LoadData()
    {
        try
        {
            var data = File.ReadAllText("nonexistent.txt");
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }
    }
}
```

### Production Mode

```csharp
// In development (Unity Editor or Development Build)
Log.Debug("Debug message");  
Log.Info("Info message");    
Log.Error("Error");          

// In production (Release Build)
Log.Debug("Debug message");  
Log.Info("Info message");    
Log.Error("Error");          
```

Production mode is **automatically detected**:
- Unity Editor → Development Mode
- Development Build → Development Mode
- Release Build → Production Mode

### Force Production Mode (Manual)

```csharp
public class GameSettings : MonoBehaviour
{
    private void Awake()
    {
        Log.IsProductionMode = true;
        
        Log.IsProductionMode = Application.isEditor ? false : true;
    }
}
```

## In-Game Console

### Open / Close

Press **`~`** (BackQuote) or the configured key to toggle the console.

### Filters

#### By Severity Level
- **Debug**: Shows Debug and all higher levels
- **Info**: Shows Info, Warning, Error, Fatal
- **Warning**: Shows Warning, Error, Fatal
- **Error**: Shows Error and Fatal
- **Fatal**: Shows only Fatal

#### By Search
Type text in the "Search" field to filter:
- By message: "player"
- By class: "PlayerController"

### Actions

- **Clear Console**: Clears all logs
- **Clear Search**: Resets the search filter
- **Details**: Click the "Details" button of a log to view full information in the Unity console

### Configuration

In the `InGameConsole` component Inspector:

| Parameter | Description | Default |
|-----------|-------------|--------|
| Toggle Key | Key to open/close | BackQuote (~) |
| Start Visible | Console visible on startup | false |
| Max Visible Logs | Maximum number of displayed logs | 100 |
| Font Size | Font size | 14 |

## Complete API

### Logging Methods

```csharp
Log.Debug(object message)
Log.Info(object message)
Log.Warning(object message)
Log.Error(object message)
Log.Fatal(object message)

Log.Exception(Exception ex)
```

### Properties

```csharp
bool Log.IsProductionMode { get; set; }

IReadOnlyList<LogEntry> Log.History { get; }
```

### Utility Methods

```csharp
Log.Clear();

Log.OnLogAdded += (LogEntry entry) => {
    // Your code here
};
```

### LogEntry (Metadata)

```csharp
public class LogEntry
{
    LogLevel Level
    string Message
    string CallerClass
    string CallerMethod
    int LineNumber
    DateTime Timestamp
    string StackTrace
    
    string GetFormattedMessage()
    string GetDetailedMessage()
    Color GetColor()
}
```

## Advanced Examples

### Custom Log Viewer

```csharp
public class LogViewer : MonoBehaviour
{
    private void OnEnable()
    {
        Log.OnLogAdded += OnNewLog;
    }

    private void OnDisable()
    {
        Log.OnLogAdded -= OnNewLog;
    }

    private void OnNewLog(LogEntry entry)
    {
        Debug.Log($"New log: {entry.GetDetailedMessage()}");
    }
}
```

### Log Statistics

```csharp
public class LogStats : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var history = Log.History;
            var errors = history.Count(l => l.Level >= LogLevel.Error);
            var warnings = history.Count(l => l.Level == LogLevel.Warning);
            
            Log.Info($"Stats - Total: {history.Count}, Errors: {errors}, Warnings: {warnings}");
        }
    }
}
```

### Save Logs to File

```csharp
public class LogSaver : MonoBehaviour
{
    public void SaveLogsToFile()
    {
        var logs = Log.History;
        var lines = logs.Select(l => l.GetDetailedMessage());
        
        var path = Application.persistentDataPath + "/logs.txt";
        File.WriteAllLines(path, lines);
        
        Log.Info($"Logs saved to {path}");
    }
}
```

## Important Notes

### Performance

- Log history is limited to **1000 logs** to prevent memory issues
- In production, Debug/Info/Warning logs are completely ignored
- The in-game console limits display to **100 logs** by default

### Thread Safety

The logging system is thread-safe thanks to the use of locks.

### Conditional Compilation

- `Log.Debug()` uses `[Conditional("DEBUG")]` and is completely removed in Release
- Other levels check `IsProductionMode` at runtime

## Troubleshooting

### Console does not appear
Verify that the `InGameConsole` component is attached to an active GameObject  
Check the configured shortcut key

### Logs do not appear in build
Normal behavior in Release Build (production mode)  
Errors still appear  
Use Development Build to keep all logs

### Class prefixes show "Unknown"
Ensure your C# file names match their class names  
The system uses the file name, not the class name

### Too many logs slow down the game
Reduce `Max Visible Logs` in the Inspector  
Use filters to show only important logs  
Call `Log.Clear()` regularly
