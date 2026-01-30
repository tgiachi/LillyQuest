using LillyQuest.Scripting.Lua.Loaders;

namespace LillyQuest.Tests.Scripting.Lua;

public class LuaScriptLoaderTests
{
    [Test]
    public void ResolveModulePath_UsesEngineDirectoryBeforePluginDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var engineDir = Path.Combine(root, "engine");
        var pluginDir = Path.Combine(root, "plugin");

        Directory.CreateDirectory(engineDir);
        Directory.CreateDirectory(pluginDir);

        File.WriteAllText(Path.Combine(pluginDir, "foo.lua"), "return 'plugin'");

        var loader = new LuaScriptLoader(new[] { engineDir, pluginDir });

        var exists = loader.ScriptFileExists("foo");
        Assert.That(exists, Is.True);

        var content = loader.LoadFile("foo", new(new())) as string;
        Assert.That(content, Does.Contain("plugin"));

        Directory.Delete(root, true);
    }
}
