// See https://aka.ms/new-console-template for more information

using ConsoleAppFramework;
using DryIoc;
using LillyQuest.Engine;
using LillyQuest.Engine.Extensions;
using LillyQuest.Engine.Logging;
using LillyQuest.Game.Scenes;
using LillyQuest.RogueLike;
using Serilog;

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
        var rootDirectory = Environment.GetEnvironmentVariable("LILYQUEST_ROOT") ?? Directory.GetCurrentDirectory();

        var lillyQuestBootstrap = new LillyQuestBootstrap(
            new()
            {
                IsDebugMode = true,
                RootDirectory =  rootDirectory,
            }
        );

        lillyQuestBootstrap.Initialize();

        // Register scenes
        lillyQuestBootstrap.RegisterServices(
            container =>
            {
                container.RegisterInstance<ILogEventDispatcher>(logDispatcher);
                container.RegisterScene<TestSceneA>();
                container.RegisterScene<TestSceneB>();
                container.RegisterScene<TilesetSurfaceEditorScene>(true);
                container.RegisterScene<UiWidgetsDemoScene>();

                container.RegisterPlugin(typeof(LillyQuestRogueLikePlugin).Assembly);

                return container;
            }
        );

        lillyQuestBootstrap.Run();
    }
);
