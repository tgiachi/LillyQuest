using DryIoc;
using LillyQuest.Core.Extensions.Container;
using LillyQuest.Core.Internal.Data.Registrations;

namespace LillyQuest.Engine.Extensions;

public static class SceneManagerExtensions
{
    /// <summary>
    /// Registers a scene type in the container and adds it to the scene registration list.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="isInitial"></param>
    /// <typeparam name="TScene"></typeparam>
    /// <returns></returns>
    public static IContainer RegisterScene<TScene>(this IContainer container, bool isInitial = false)
    {
        container.Register<TScene>(Reuse.Singleton);
        container.AddToRegisterTypedList(new SceneRegistrationObject(typeof(TScene), isInitial));

        return container;
    }
}
