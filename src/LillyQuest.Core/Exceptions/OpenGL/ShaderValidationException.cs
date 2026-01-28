namespace LillyQuest.Core.Exceptions.OpenGL;

public sealed class ShaderValidationException : ShaderException
{
    public ShaderValidationException(string message) : base(message) { }
}
