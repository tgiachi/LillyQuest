# Plugin Lifecycle & Async Resource Loading

## Overview

The LillyQuest Engine provides a plugin lifecycle system with three main hooks:
- **OnEngineReady()** - Engine initialized, no rendering yet
- **OnReadyToRender()** - Window created, OpenGL available
- **OnLoadResources()** - Resources being loaded

This guide shows how to use **non-blocking async loading** to prevent freezing the render loop.

## The Problem: Blocking Render Loop

❌ **DON'T do this** - It will freeze the screen:

```csharp
public async Task OnLoadResources(IContainer container)
{
    // This blocks the main render thread!
    await Task.Delay(1000);
    Log.Information("Loading resources...");
    await Task.Delay(1000);
}
```

The `await Task.Delay()` executes in the **main thread** of Silk.NET's game loop, which also handles rendering. While waiting, the render loop is blocked and the screen freezes.

## Solution: Async Background Loading

✅ **DO this** - The render loop continues while loading happens in background:

```csharp
public async Task OnLoadResources(IContainer container)
{
    var asyncLoader = container.Resolve<AsyncResourceLoader>();

    // Start loading in background WITHOUT blocking the render loop
    asyncLoader.StartAsyncLoading(async () =>
    {
        Log.Information("Loading RogueLike plugin resources...");

        // This runs in a background task - render loop is NOT blocked
        await Task.Delay(1000);

        // Load your actual resources here
        var assetManager = container.Resolve<IAssetManager>();
        // assetManager.LoadAsset(...);

        await Task.Delay(1000);

        Log.Information("✓ RogueLike plugin resources loaded");
    });

    // Return IMMEDIATELY - render loop continues!
    // The LogScreen shows messages in real-time
}
```

## How It Works

1. **OnLoadResources()** called in main thread - returns immediately
2. **asyncLoader.StartAsyncLoading()** starts a background `Task.Run()`
3. **Render loop continues** - game keeps rendering smoothly
4. **Background task runs** - your plugin loads resources
5. **LogScreen updates in real-time** - shows log messages from the plugin
6. **When complete** - the plugin is fully loaded

## Real-World Example: RogueLike Plugin

```csharp
public class LillyQuestRogueLikePlugin : ILillyQuestPlugin
{
    public PluginInfo PluginInfo => new(
        Id: "com.github.tgiachi.lillyquest.roguelike",
        Name: "LillyQuest RogueLike",
        Version: "1.0.0",
        Author: "TGIACHI",
        Description: "RogueLike module for LillyQuest",
        Dependencies: []
    );

    public async Task OnEngineReady(IContainer container)
    {
        Log.Information("RogueLike plugin engine ready");
        await Task.CompletedTask;
    }

    public async Task OnReadyToRender(IContainer container)
    {
        Log.Information("RogueLike plugin graphics ready");
        // Setup graphics resources here
        await Task.CompletedTask;
    }

    public async Task OnLoadResources(IContainer container)
    {
        var asyncLoader = container.Resolve<AsyncResourceLoader>();

        asyncLoader.StartAsyncLoading(async () =>
        {
            Log.Information("Loading RogueLike resources...");

            // Simulate loading various components
            for (int i = 0; i < 5; i++)
            {
                Log.Information($"  - Loading component {i + 1}/5...");
                await Task.Delay(500);
            }

            Log.Information("✓ RogueLike resources fully loaded");
        });
    }

    public void RegisterServices(IContainer container) { }
    public void Shutdown() { }
}
```

## Monitoring Loading Progress

### Option 1: Check if Loading is Complete

```csharp
// In your game loop or scene:
var bootstrap = container.Resolve<LillyQuestBootstrap>();

while (bootstrap.IsLoadingResources)
{
    // Keep showing the LogScreen
    // The renderer updates automatically
}

// When done, show initial scene
```

### Option 2: Wait for Loading to Complete

```csharp
var bootstrap = container.Resolve<LillyQuestBootstrap>();

// Show LogScreen
sceneManager.TransitionToScene<LogScene>();

// Wait for all background loading to finish
await bootstrap.WaitForResourcesLoaded();

// Show initial scene
sceneManager.TransitionToScene<InitialScene>();
```

## Best Practices

1. **Keep OnLoadResources() fast** - just start the async loading, don't wait for it
2. **Do real work in the background task** - CPU-intensive operations should be in the Task.Run()
3. **Use Log.Information()** - the LogScreen will show all messages in real-time
4. **Load graphics resources from main thread** - If you need OpenGL operations, do them in OnReadyToRender() or schedule them back to main thread
5. **Handle exceptions** - AsyncResourceLoader logs errors automatically

## Architecture

```
Run()
  ├─ ExecuteOnEngineReady() [blocks - but fast]
  ├─ _window.Run() [main game loop starts]
  │   ├─ WindowOnLoad() [called once]
  │   │   ├─ OpenGL init
  │   │   ├─ ExecuteOnReadyToRender() [blocks - but fast]
  │   │   └─ StartInternalServices()
  │   │
  │   └─ Game Loop [repeats every frame]
  │       ├─ Handle Input
  │       ├─ Update Logic
  │       ├─ Check IsLoadingResources [non-blocking]
  │       └─ Render [includes LogScreen if loading]
  │
  └─ When loading complete:
      └─ Transition to initial scene
```

## Troubleshooting

**Problem: Screen still freezes**
- Make sure you're using `asyncLoader.StartAsyncLoading()`
- Don't `await` the asyncLoader - just call `StartAsyncLoading()` and return

**Problem: Resources not loaded**
- Remember to `await bootstrap.WaitForResourcesLoaded()` before using the resources
- Or check `bootstrap.IsLoadingResources` to know when it's done

**Problem: LogScreen not updating**
- Make sure you're using `Log.Information()` from Serilog
- The LogScreen reads from the log dispatcher in real-time

## See Also

- `AsyncResourceLoader.cs` - Implementation
- `ILillyQuestPlugin.cs` - Plugin interface
- `PluginLifecycleExecutor.cs` - Hook execution
- `LillyQuestBootstrap.cs` - Bootstrap initialization
