using LillyQuest.Scripting.Lua.Attributes;
using LillyQuest.Scripting.Lua.Utils;

namespace LillyQuest.Tests.Scripting.Lua;

[TestFixture]
public class LuaDocumentationGeneratorTests
{
    private sealed class TestLuaFieldClass
    {
        [LuaField("hp")]
        public int HitPoints { get; set; }
    }

    [Test]
    public void GenerateDocumentation_UsesLuaFieldNameForProperties()
    {
        LuaDocumentationGenerator.AddClassToGenerate(typeof(TestLuaFieldClass));

        var output = LuaDocumentationGenerator.GenerateDocumentation(
            "TestApp",
            "1.0",
            new(),
            new(),
            new()
        );

        Assert.That(output, Does.Contain("---@field hp number"));
    }

    [SetUp]
    public void SetUp()
    {
        LuaDocumentationGenerator.ClearCaches();
    }
}
