// See https://aka.ms/new-console-template for more information

using ConsoleAppFramework;
using LillyQuest.Engine;
using Serilog;

var loggerConfiguration = new LoggerConfiguration().WriteTo.Console();

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
