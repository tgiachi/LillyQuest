// See https://aka.ms/new-console-template for more information

using ConsoleAppFramework;
using LillyQuest.Engine;
using LillyQuest.Engine.Extensions;
using LillyQuest.Engine.Logging;
using LillyQuest.Game.Scenes;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var logDispatcher = new LogEventDispatcher();

Log.Logger = new LoggerConfiguration()
             .MinimumLevel
             .Debug()
             .WriteTo
             .Console()
             .WriteTo
             .Sink(new LogEventBufferSink(logDispatcher))
             .CreateLogger();

await ConsoleApp.RunAsync(
    args,
    () =>
    {
        var lillyQuestBootstrap = new LillyQuestBootstrap(
            new()
            {
                IsDebugMode = true
            }
        );

        lillyQuestBootstrap.Initialize();

        // Register scenes
        lillyQuestBootstrap.RegisterServices(
            container =>
            {
                container.RegisterScene<TestSceneA>(true);
                container.RegisterScene<TestSceneB>();
                container.RegisterScene<TilesetSurfaceEditorScene>();

                return container;
            }
        );

        lillyQuestBootstrap.Run();
    }
);
