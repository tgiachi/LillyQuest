namespace LillyQuest.Core.Exceptions.OpenGL;

public sealed class ShaderUniformNotFoundException : ShaderException
{
    public ShaderUniformNotFoundException(string message) : base(message) { }
}
