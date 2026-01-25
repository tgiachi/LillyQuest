namespace LillyQuest.Core.Data.Directories;

public sealed class DirectorySearchResult
{
    public string Path { get; }
    public string Name { get; }
    public string Extension { get; }
    public long SizeBytes { get; }
    public DateTime LastModifiedUtc { get; }

    public DirectorySearchResult(string path, string name, string extension, long sizeBytes, DateTime lastModifiedUtc)
    {
        Path = path;
        Name = name;
        Extension = extension;
        SizeBytes = sizeBytes;
        LastModifiedUtc = lastModifiedUtc;
    }
}
