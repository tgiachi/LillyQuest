using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Interfaces.Systems;

/// <summary>
/// Defines a game system that processes entities for a specific query type and order.
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Gets the execution order within the system list for the same query type.
    /// </summary>
    uint Order { get; }

    /// <summary>
    /// Gets the system name used for diagnostics and logging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the query types that determine when this system runs.
    /// </summary>
    SystemQueryType QueryType { get; }

    /// <summary>
    /// Initializes the system before it starts processing entities.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Processes entities for the current tick of the specified query type.
    /// </summary>
    void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager);
}
