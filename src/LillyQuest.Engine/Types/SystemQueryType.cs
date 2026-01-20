namespace LillyQuest.Engine.Types;

[Flags]
public enum SystemQueryType
{
    None,
    Updateable,
    FixedUpdateable,
    Renderable,
    DebugRenderable
}
