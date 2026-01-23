using Silk.NET.Maths;

namespace LillyQuest.Core.Data.Assets;

public readonly struct TexturePatch
{
    public string TextureName { get; }
    public string ElementName { get; }
    public Rectangle<int> Section { get; }

    public TexturePatch(string textureName, string elementName, Rectangle<int> section)
    {
        TextureName = textureName;
        ElementName = elementName;
        Section = section;
    }
}
