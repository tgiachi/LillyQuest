using LillyQuest.Scripting.Lua.Extensions;
using MoonSharp.Interpreter;
using NUnit.Framework;

namespace LillyQuest.Tests.Scripting.Lua;

public class LuaTableReaderTests
{
    private enum DemoEnum
    {
        Low = 1,
        High = 2
    }

    [Test]
    public void GetInt_ReturnsDefaultWhenMissing()
    {
        var table = new Table(new Script());

        Assert.That(LuaTableReader.GetInt(table, "hp", 7), Is.EqualTo(7));
    }

    [Test]
    public void GetString_ReadsStringValue()
    {
        var table = new Table(new Script()) { ["name"] = "Lua" };

        Assert.That(LuaTableReader.GetString(table, "name"), Is.EqualTo("Lua"));
    }

    [Test]
    public void GetFloat_ReadsNumberValue()
    {
        var table = new Table(new Script()) { ["speed"] = 2.5 };

        Assert.That(LuaTableReader.GetFloat(table, "speed"), Is.EqualTo(2.5f));
    }

    [Test]
    public void GetBool_ReadsBooleanValue()
    {
        var table = new Table(new Script()) { ["alive"] = true };

        Assert.That(LuaTableReader.GetBool(table, "alive"), Is.True);
    }

    [Test]
    public void GetEnum_ReadsEnumByString()
    {
        var table = new Table(new Script()) { ["tier"] = "High" };

        Assert.That(LuaTableReader.GetEnum(table, "tier", DemoEnum.Low), Is.EqualTo(DemoEnum.High));
    }

    [Test]
    public void GetEnum_ReadsEnumByNumber()
    {
        var table = new Table(new Script()) { ["tier"] = 1 };

        Assert.That(LuaTableReader.GetEnum(table, "tier", DemoEnum.High), Is.EqualTo(DemoEnum.Low));
    }
}
