using DarkLilly.Engine.Interfaces.GameObjects.Features.Base;
using Silk.NET.Input;

namespace DarkLilly.Engine.Interfaces.GameObjects.Features.Input;

public interface IMouseInputFeature : IGameObjectFeature
{
    void OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons);
    void OnMouseMove(int x, int y);
    void OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons);
    void OnMouseWheel(int x, int y, float delta);
}
