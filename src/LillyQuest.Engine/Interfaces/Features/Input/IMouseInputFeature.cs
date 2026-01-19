using LillyQuest.Engine.Interfaces.GameObjects.Features;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Features.Input;

public interface IMouseInputFeature : IGameObjectFeature
{
    void OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons);
    void OnMouseMove(int x, int y);
    void OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons);
    void OnMouseWheel(int x, int y, float delta);
}
