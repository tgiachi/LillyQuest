using Silk.NET.Maths;

namespace LillyQuest.Core.Data.Assets;

public readonly struct TexturePatchDefinition
{
    public string ElementName { get; }
    public Rectangle<int> Section { get; }

    public TexturePatchDefinition(string elementName, Rectangle<int> section)
    {
        ElementName = elementName;
        Section = section;
    }
}
