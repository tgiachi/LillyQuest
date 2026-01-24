using System;
using System.IO;
using DryIoc;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Scripting.Lua.Data.Config;
using LillyQuest.Scripting.Lua.Services;
using MoonSharp.Interpreter;
using NUnit.Framework;

namespace LillyQuest.Tests.Scripting.Lua;

public class LuaScriptEngineServiceTests
{
    [Test]
    public void AddSearchDirectory_AllowsRequireFromPluginDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var engineDir = Path.Combine(root, "engine");
        var pluginDir = Path.Combine(root, "plugin");

        Directory.CreateDirectory(engineDir);
        Directory.CreateDirectory(pluginDir);

        File.WriteAllText(Path.Combine(pluginDir, "foo.lua"), "return 'plugin'");

        var directoriesConfig = new DirectoriesConfig(root, Array.Empty<string>());
        var container = new Container();
        var engineConfig = new LuaEngineConfig(Path.Combine(root, "luarc"), engineDir, "test");
        using var service = new LuaScriptEngineService(directoriesConfig, container, engineConfig);

        service.AddSearchDirectory(pluginDir);

        var result = service.LuaScript.DoString("return require('foo')");
        Assert.That(result.String, Is.EqualTo("plugin"));

        Directory.Delete(root, recursive: true);
    }
}
