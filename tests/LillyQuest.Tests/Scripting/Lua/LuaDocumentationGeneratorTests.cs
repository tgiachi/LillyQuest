using System.Collections.Generic;
using LillyQuest.Scripting.Lua.Attributes;
using LillyQuest.Scripting.Lua.Data.Internal;
using LillyQuest.Scripting.Lua.Utils;
using NUnit.Framework;

namespace LillyQuest.Tests.Scripting.Lua;

[TestFixture]
public class LuaDocumentationGeneratorTests
{
    private sealed class TestLuaFieldClass
    {
        [LuaField("hp")]
        public int HitPoints { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        LuaDocumentationGenerator.ClearCaches();
    }

    [Test]
    public void GenerateDocumentation_UsesLuaFieldNameForProperties()
    {
        LuaDocumentationGenerator.AddClassToGenerate(typeof(TestLuaFieldClass));

        var output = LuaDocumentationGenerator.GenerateDocumentation(
            "TestApp",
            "1.0",
            new List<ScriptModuleData>(),
            new Dictionary<string, object>(),
            new Dictionary<string, IReadOnlyCollection<string>>()
        );

        Assert.That(output, Does.Contain("---@field hp number"));
    }
}
