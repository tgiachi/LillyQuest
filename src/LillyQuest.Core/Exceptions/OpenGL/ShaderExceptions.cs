namespace LillyQuest.Core.Exceptions.OpenGL;

public class ShaderException : Exception
{
    public ShaderException(string message) : base(message) { }
}

public sealed class ShaderCompileException : ShaderException
{
    public ShaderCompileException(string message) : base(message) { }
}

public sealed class ShaderLinkException : ShaderException
{
    public ShaderLinkException(string message) : base(message) { }
}

public sealed class ShaderUniformNotFoundException : ShaderException
{
    public ShaderUniformNotFoundException(string message) : base(message) { }
}

public sealed class ShaderValidationException : ShaderException
{
    public ShaderValidationException(string message) : base(message) { }
}
