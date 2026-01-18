// See https://aka.ms/new-console-template for more information

using LillyQuest.Core.Data.Configs;
using LillyQuest.Engine;

Console.WriteLine("Hello, World!");
var lillyQuestBootstrap = new LillyQuestBootstrap(new LillyQuestEngineConfig());

lillyQuestBootstrap.Initialize();
lillyQuestBootstrap.Run();
