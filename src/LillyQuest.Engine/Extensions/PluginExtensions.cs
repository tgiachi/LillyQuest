using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DryIoc;
using LillyQuest.Core.Data.Plugins;
using LillyQuest.Core.Extensions.Container;
using LillyQuest.Engine.Interfaces.Plugins;

namespace LillyQuest.Engine.Extensions;

public static class PluginExtensions
{
    [RequiresUnreferencedCode("Plugin registration requires reflection on assemblies which may not be trimmed.")]
    public static IContainer RegisterPlugin(this IContainer container, Assembly assembly)
    {
        var pluginTypes = assembly.GetTypes()
                                  .Where(
                                      t => typeof(ILillyQuestPlugin).IsAssignableFrom(t) &&
                                           !t.IsInterface &&
                                           !t.IsAbstract
                                  );

        foreach (var pluginType in pluginTypes)
        {
            container.AddToRegisterTypedList(new EnginePluginRegistration(assembly, pluginType));
            container.Register(pluginType, Reuse.Singleton);
        }

        return container;
    }
}
