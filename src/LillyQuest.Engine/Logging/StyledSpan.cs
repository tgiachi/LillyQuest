using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Logging;

public sealed record StyledSpan(
    string Text,
    LyColor Foreground,
    LyColor? Background,
    bool Bold,
    bool Italic,
    bool Underline
);
