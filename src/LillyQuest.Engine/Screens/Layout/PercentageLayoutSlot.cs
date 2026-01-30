using System.Numerics;
using LillyQuest.Engine.Interfaces.Screens;

namespace LillyQuest.Engine.Screens.Layout;

public readonly record struct PercentageLayoutSlot(
    IScreen Screen,
    Vector4 PercentRect,
    Vector2? MinSize,
    Vector2? MaxSize
);
