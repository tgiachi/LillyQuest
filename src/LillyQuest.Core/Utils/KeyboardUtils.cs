using LillyQuest.Core.Types;
using Silk.NET.Input;

namespace LillyQuest.Core.Utils;

/// <summary>
/// Provides helper methods to translate keyboard modifiers and input text to characters.
/// </summary>
public static class KeyboardUtils
{
    /// <summary>
    /// Returns a character based on modifier state and provided texts.
    /// </summary>
    /// <param name="isShiftPressed">True when the Shift modifier is active.</param>
    /// <param name="normalText">The base text for the key (e.g., "q").</param>
    /// <param name="shiftedText">The shifted text for the key (e.g., "Q" or "!").</param>
    /// <returns>The resolved character, or null when no valid text is provided.</returns>
    public static char? GetCharacter(bool isShiftPressed, string? normalText, string? shiftedText)
    {
        if (isShiftPressed && !string.IsNullOrEmpty(shiftedText))
        {
            return shiftedText[0];
        }

        if (string.IsNullOrEmpty(normalText))
        {
            return null;
        }

        var character = normalText[0];

        return isShiftPressed ? char.ToUpperInvariant(character) : character;
    }

    /// <summary>
    /// Parses a shortcut string like "CTRL+ALT+A" into modifier and key.
    /// </summary>
    /// <param name="shortcut">Shortcut string with "+" separators.</param>
    /// <param name="modifier">Parsed modifier flags.</param>
    /// <param name="key">Parsed main key.</param>
    /// <returns>True when parsing succeeds; otherwise false.</returns>
    public static bool TryParseShortcut(string? shortcut, out KeyModifierType modifier, out Key key)
    {
        modifier = KeyModifierType.None;
        key = default;

        if (string.IsNullOrWhiteSpace(shortcut))
        {
            return false;
        }

        var parts = shortcut.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return false;
        }

        var keyAssigned = false;

        foreach (var part in parts)
        {
            if (IsModifierToken(part, out var modifierValue))
            {
                modifier |= modifierValue;

                continue;
            }

            if (keyAssigned)
            {
                return false;
            }

            if (!Enum.TryParse(part, true, out Key parsedKey))
            {
                return false;
            }

            key = parsedKey;
            keyAssigned = true;
        }

        return keyAssigned;
    }

    private static bool IsModifierToken(string token, out KeyModifierType modifier)
    {
        modifier = KeyModifierType.None;

        switch (token.ToUpperInvariant())
        {
            case "SHIFT":
                modifier = KeyModifierType.Shift;

                return true;
            case "CTRL":
            case "CONTROL":
                modifier = KeyModifierType.Control;

                return true;
            case "ALT":
                modifier = KeyModifierType.Alt;

                return true;
            case "META":
            case "CMD":
            case "COMMAND":
            case "WIN":
            case "SUPER":
                modifier = KeyModifierType.Meta;

                return true;
            default:
                return false;
        }
    }
}
