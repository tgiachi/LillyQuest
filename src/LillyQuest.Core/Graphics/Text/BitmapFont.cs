using LillyQuest.Core.Graphics.OpenGL.Resources;

namespace LillyQuest.Core.Graphics.Text;

/// <summary>
/// Represents a bitmap font stored in a texture atlas with fixed-size tiles.
/// </summary>
public sealed class BitmapFont
{
    public string Name { get; }
    public Texture2D Texture { get; }
    public int TileWidth { get; }
    public int TileHeight { get; }
    public int Spacing { get; }
    public int Columns { get; }
    public int Rows { get; }
    public string? CharacterMap { get; }

    private readonly Dictionary<char, int>? _characterLookup;

    public int GlyphCount => Columns * Rows;

    public BitmapFont(
        string name,
        Texture2D texture,
        int tileWidth,
        int tileHeight,
        int spacing,
        string? characterMap = null
    )
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileHeight);

        Name = name;
        Texture = texture;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Spacing = spacing;
        CharacterMap = characterMap;

        Columns = texture.Width / tileWidth;
        Rows = texture.Height / tileHeight;

        if (Columns <= 0 || Rows <= 0)
        {
            throw new ArgumentException("Tile size is larger than the bitmap texture.", nameof(tileWidth));
        }

        if (!string.IsNullOrEmpty(characterMap))
        {
            _characterLookup = new(characterMap.Length);

            for (var i = 0; i < characterMap.Length; i++)
            {
                var ch = characterMap[i];

                if (!_characterLookup.ContainsKey(ch))
                {
                    _characterLookup[ch] = i;
                }
            }
        }
    }

    public bool TryGetGlyphIndex(char ch, out int index)
    {
        if (_characterLookup != null)
        {
            return _characterLookup.TryGetValue(ch, out index);
        }

        index = ch;

        return true;
    }
}
