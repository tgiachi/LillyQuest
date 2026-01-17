using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Data.Json.Converters;

/// <summary>
/// Converts LyColor values from JSON in hex string format (#AARRGGBB).
/// </summary>
public sealed class LyColorConverter : JsonConverter<LyColor>
{
    /// <summary>
    /// Reads a LyColor from JSON values like "#AARRGGBB".
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The target type.</param>
    /// <param name="options">Serializer options.</param>
    /// <returns>The parsed LyColor.</returns>
    public override LyColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            return string.IsNullOrWhiteSpace(value)
                       ? throw new JsonException("LyColor string value cannot be null or empty.")
                       : ReadFromHex(value);
        }

        throw new JsonException($"Unsupported LyColor token type: {reader.TokenType}.");
    }

    /// <summary>
    /// Writes a LyColor as a hex string "#AARRGGBB".
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The LyColor value.</param>
    /// <param name="options">Serializer options.</param>
    public override void Write(Utf8JsonWriter writer, LyColor value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}");
    }

    private static LyColor ReadFromHex(string value)
    {
        var text = value.Trim();

        if (text.StartsWith('#'))
        {
            text = text[1..];
        }

        if (text.Length != 8)
        {
            throw new JsonException("LyColor hex string must be in #AARRGGBB format.");
        }

        if (!TryReadHexByte(text, 0, out var a) ||
            !TryReadHexByte(text, 2, out var r) ||
            !TryReadHexByte(text, 4, out var g) ||
            !TryReadHexByte(text, 6, out var b))
        {
            throw new JsonException("LyColor hex string contains invalid characters.");
        }

        return new(a, r, g, b);
    }

    private static bool TryReadHexByte(string text, int start, out byte value)
        => byte.TryParse(
            text.AsSpan(start, 2),
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out value
        );
}
