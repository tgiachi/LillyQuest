// See https://aka.ms/new-console-template for more information

using ConsoleAppFramework;
using LillyQuest.Engine;
using LillyQuest.Engine.Extensions;
using LillyQuest.Game.Scenes;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(theme: AnsiConsoleTheme.Code);

Log.Logger = loggerConfiguration.CreateLogger();

await ConsoleApp.RunAsync(
    args,
    () =>
    {
        var lillyQuestBootstrap = new LillyQuestBootstrap(new());

        lillyQuestBootstrap.Initialize();

        // Register scenes
        lillyQuestBootstrap.RegisterServices(
            container =>
            {
                container.RegisterScene<TestSceneA>(true);
                container.RegisterScene<TestSceneB>();

                return container;
            }
        );

        lillyQuestBootstrap.Run();
    }
);
