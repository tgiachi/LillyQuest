using DarkLilly.Core.Types;
using DarkLilly.Engine.Interfaces.GameObjects.Features.Base;
using Silk.NET.Input;

namespace DarkLilly.Engine.Interfaces.GameObjects.Features.Input;

public interface IKeyboardInputFeature : IGameObjectFeature
{
    void OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> key);
    void OnKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> key);
    void OnKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> key);
}
