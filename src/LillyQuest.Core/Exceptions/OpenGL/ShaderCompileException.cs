namespace LillyQuest.Core.Exceptions.OpenGL;

public sealed class ShaderCompileException : ShaderException
{
    public ShaderCompileException(string message) : base(message) { }
}
