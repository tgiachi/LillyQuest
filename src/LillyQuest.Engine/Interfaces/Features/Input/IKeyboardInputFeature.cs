using LillyQuest.Core.Types;
using LillyQuest.Engine.Interfaces.GameObjects.Features;
using Silk.NET.Input;

namespace LillyQuest.Engine.Interfaces.Features.Input;

public interface IKeyboardInputFeature : IGameObjectFeature
{
    void OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> key);
    void OnKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> key);
    void OnKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> key);
}
