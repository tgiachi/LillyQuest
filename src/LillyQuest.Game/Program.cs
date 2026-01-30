// See https://aka.ms/new-console-template for more information

using ConsoleAppFramework;
using DryIoc;
using LillyQuest.Engine;
using LillyQuest.Engine.Extensions;
using LillyQuest.Engine.Logging;
using LillyQuest.Game.Scenes;
using LillyQuest.RogueLike;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var logDispatcher = new LogEventDispatcher();

Log.Logger = new LoggerConfiguration()
             .MinimumLevel
             .Debug()
             .WriteTo
             .Console(theme: AnsiConsoleTheme.Sixteen)
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
                RootDirectory = rootDirectory
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
                container.RegisterScene<TilesetSurfaceEditorScene>();
                container.RegisterScene<UiWidgetsDemoScene>();
                container.RegisterScene<UiMenuDemoScene>();
                container.RegisterScene<UiTextBoxDemoScene>();
                container.RegisterScene<RogueScene>(true);

                container.RegisterPlugin(typeof(LillyQuestRogueLikePlugin).Assembly);

                return container;
            }
        );

        lillyQuestBootstrap.Run();
    }
);
