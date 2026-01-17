namespace LillyQuest.Core.Exceptions.Models;

public class ModelLoadException : Exception
{
    public ModelLoadException(string message) : base(message) { }
}
