namespace LillyQuest.Core.Data.Json.Assets;

public class TilesetDefinitionJson
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }

    public int Spacing { get; set; }
    public int Margin { get; set; }

    public string ImagePath { get; set; }

    public override string ToString()
        => $"Tileset: {ImagePath} ({TileWidth}x{TileHeight}, Spacing: {Spacing}, Margin: {Margin})";
}
