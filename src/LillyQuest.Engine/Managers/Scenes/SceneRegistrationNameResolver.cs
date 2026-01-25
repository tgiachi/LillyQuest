using DryIoc;
using LillyQuest.Core.Internal.Data.Registrations;
using LillyQuest.Engine.Interfaces.Scenes;

namespace LillyQuest.Engine.Managers.Scenes;

public static class SceneRegistrationNameResolver
{
    public static IReadOnlyList<string> ResolveRegisteredSceneNames(
        IReadOnlyList<SceneRegistrationObject> registrations,
        IContainer container
    )
    {
        if (registrations.Count == 0)
        {
            return Array.Empty<string>();
        }

        var names = new List<string>(registrations.Count);

        foreach (var registration in registrations)
        {
            var scene = (IScene)container.Resolve(registration.SceneType);
            names.Add(scene.Name);
        }

        return names.AsReadOnly();
    }
}
