using LillyQuest.Scripting.Lua.Extensions;
using MoonSharp.Interpreter;

namespace LillyQuest.Tests.Scripting.Lua;

public class LuaTableReaderTests
{
    [Test]
    public void GetBool_ReadsBooleanValue()
    {
        var table = new Table(new()) { ["alive"] = true };

        Assert.That(LuaTableReader.GetBool(table, "alive"), Is.True);
    }

    [Test]
    public void GetEnum_ReadsEnumByNumber()
    {
        var table = new Table(new()) { ["tier"] = 1 };

        Assert.That(LuaTableReader.GetEnum(table, "tier", DemoEnum.High), Is.EqualTo(DemoEnum.Low));
    }

    [Test]
    public void GetEnum_ReadsEnumByString()
    {
        var table = new Table(new()) { ["tier"] = "High" };

        Assert.That(LuaTableReader.GetEnum(table, "tier", DemoEnum.Low), Is.EqualTo(DemoEnum.High));
    }

    [Test]
    public void GetFloat_ReadsNumberValue()
    {
        var table = new Table(new()) { ["speed"] = 2.5 };

        Assert.That(LuaTableReader.GetFloat(table, "speed"), Is.EqualTo(2.5f));
    }

    [Test]
    public void GetInt_ReturnsDefaultWhenMissing()
    {
        var table = new Table(new());

        Assert.That(LuaTableReader.GetInt(table, "hp", 7), Is.EqualTo(7));
    }

    [Test]
    public void GetString_ReadsStringValue()
    {
        var table = new Table(new()) { ["name"] = "Lua" };

        Assert.That(LuaTableReader.GetString(table, "name"), Is.EqualTo("Lua"));
    }
}
