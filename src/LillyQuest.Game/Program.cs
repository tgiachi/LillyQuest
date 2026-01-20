// See https://aka.ms/new-console-template for more information

using ConsoleAppFramework;
using LillyQuest.Engine;
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
        lillyQuestBootstrap.Run();
    }
);
