using System.Numerics;

namespace LillyQuest.Engine.Interfaces.Screens;

public interface ILayoutAwareScreen
{
    void ApplyLayout(Vector2 position, Vector2 size, Vector2 rootSize);
}
