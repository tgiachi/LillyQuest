using LillyQuest.Core.Data.Directories;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for DirectoriesConfig directory management and path resolution
/// </summary>
public class DirectoriesConfigTests
{
    private string _testRootDirectory;

    /// <summary>
    /// Test enum for directory types
    /// </summary>
    private enum DirectoryTypeEnum
    {
        Assets,
        Config,
        Data,
        Logs
    }

    [SetUp]
    public void Setup()
    {
        // Create a unique temporary directory for each test
        _testRootDirectory = Path.Combine(Path.GetTempPath(), $"LillyQuestTest_{Guid.NewGuid()}");

        if (Directory.Exists(_testRootDirectory))
        {
            Directory.Delete(_testRootDirectory, recursive: true);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        // Clean up test directories
        if (Directory.Exists(_testRootDirectory))
        {
            Directory.Delete(_testRootDirectory, recursive: true);
        }
    }

    [Test]
    public void Constructor_WithEnumDirectories_CreatesAllDirectories()
    {
        var config = new DirectoriesConfig(_testRootDirectory, DirectoryTypeEnum.Assets, DirectoryTypeEnum.Config);

        Assert.That(Directory.Exists(Path.Combine(_testRootDirectory, "assets")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(_testRootDirectory, "config")), Is.True);
    }

    [Test]
    public void Constructor_WithExistingRoot_DoesNotFail()
    {
        // Create the root directory first
        Directory.CreateDirectory(_testRootDirectory);

        // Should not throw
        var config = new DirectoriesConfig(_testRootDirectory, "Assets");

        Assert.That(config.Root, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void Constructor_WithRootAndDirectories_CreatesRootDirectory()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets", "Config");

        Assert.That(Directory.Exists(_testRootDirectory), Is.True);
    }

    [Test]
    public void Constructor_WithStringDirectories_CreatesAllDirectories()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets", "Config", "Data");

        Assert.That(Directory.Exists(Path.Combine(_testRootDirectory, "assets")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(_testRootDirectory, "config")), Is.True);
        Assert.That(Directory.Exists(Path.Combine(_testRootDirectory, "data")), Is.True);
    }

    [Test]
    public void DirectoryName_WithSnakeCaseConversion_NormalizesCorrectly()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        // Test that CamelCase is converted to snake_case
        // MyCustomDirectory -> my_custom_directory -> my/custom/directory (nested path)
        var path = config.GetPath("MyCustomDirectory");

        Assert.That(path, Contains.Substring("my"));
        Assert.That(path, Contains.Substring("custom"));
        Assert.That(path, Contains.Substring("directory"));
    }

    [Test]
    public void GetPath_WithCaseInsensitiveRoot_ReturnsRootDirectory()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets");

        var path1 = config.GetPath("ROOT");
        var path2 = config.GetPath("root");
        var path3 = config.GetPath("Root");

        Assert.That(path1, Is.EqualTo(_testRootDirectory));
        Assert.That(path2, Is.EqualTo(_testRootDirectory));
        Assert.That(path3, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void GetPath_WithEmptyString_ReturnsRoot()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        var path = config.GetPath("");

        Assert.That(path, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void GetPath_WithEnumValue_ReturnsCorrectPath()
    {
        var config = new DirectoriesConfig(_testRootDirectory, DirectoryTypeEnum.Assets, DirectoryTypeEnum.Config);

        var assetsPath = config.GetPath(DirectoryTypeEnum.Assets);

        Assert.That(assetsPath, Is.EqualTo(Path.Combine(_testRootDirectory, "assets")));
        Assert.That(Directory.Exists(assetsPath), Is.True);
    }

    [Test]
    public void GetPath_WithExistingPath_DoesNotRecreate()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets");
        var assetsPath = config.GetPath("Assets");

        // Create a file to verify directory is not deleted
        var testFile = Path.Combine(assetsPath, "test.txt");
        File.WriteAllText(testFile, "test");

        // Get the path again
        var pathAgain = config.GetPath("Assets");

        Assert.That(File.Exists(testFile), Is.True);
        Assert.That(pathAgain, Is.EqualTo(assetsPath));
    }

    [Test]
    public void GetPath_WithMultipleUnderscores_CreatesCorrectNestedStructure()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        var path = config.GetPath("Assets_Textures_UI_Buttons");

        var expectedPath = Path.Combine(_testRootDirectory, "assets", "textures", "ui", "buttons");
        Assert.That(path, Is.EqualTo(expectedPath));
        Assert.That(Directory.Exists(path), Is.True);
    }

    [Test]
    public void GetPath_WithNestedDirectoryName_CreatesNestedStructure()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets_Textures_UI");

        var path = config.GetPath("Assets_Textures_UI");

        var expectedPath = Path.Combine(_testRootDirectory, "assets", "textures", "ui");
        Assert.That(path, Is.EqualTo(expectedPath));
        Assert.That(Directory.Exists(path), Is.True);
    }

    [Test]
    public void GetPath_WithNullString_ReturnsRoot()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        var path = config.GetPath(null);

        Assert.That(path, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void GetPath_WithPathSeparator_HandlesPathsCorrectly()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        // Paths with directory separators should be treated as-is
        var customPath = $"custom{Path.DirectorySeparatorChar}subfolder";
        var path = config.GetPath(customPath);

        Assert.That(path, Contains.Substring("custom"));
        Assert.That(path, Contains.Substring("subfolder"));
    }

    [Test]
    public void GetPath_WithRootKey_ReturnsRootDirectory()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets");

        var path = config.GetPath("Root");

        Assert.That(path, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void GetPath_WithStringKey_ReturnsCorrectPath()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets", "Config");

        var assetsPath = config.GetPath("Assets");

        Assert.That(assetsPath, Is.EqualTo(Path.Combine(_testRootDirectory, "assets")));
        Assert.That(Directory.Exists(assetsPath), Is.True);
    }

    [Test]
    public void GetPath_WithUnknownDirectory_CreatesDirectoryAutomatically()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        // NewDirectory -> new_directory -> new/directory (nested path due to underscore conversion)
        var path = config.GetPath("NewDirectory");

        Assert.That(Directory.Exists(path), Is.True);

        // The first underscore splits into nested paths
        var expectedPath = Path.Combine(_testRootDirectory, "new", "directory");
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test]
    public void GetPath_WithWhitespaceDirectoryName_ReturnsEmpty()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        var path = config.GetPath("   ");

        Assert.That(path, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void Indexer_WithEnum_ReturnsPath()
    {
        var config = new DirectoriesConfig(_testRootDirectory, DirectoryTypeEnum.Assets);

        var path = config[DirectoryTypeEnum.Assets];

        Assert.That(path, Is.EqualTo(Path.Combine(_testRootDirectory, "assets")));
    }

    [Test]
    public void Indexer_WithString_ReturnsPath()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets");

        var path = config["Assets"];

        Assert.That(path, Is.EqualTo(Path.Combine(_testRootDirectory, "assets")));
    }

    [Test]
    public void Root_Property_ReturnsRootDirectory()
    {
        var config = new DirectoriesConfig(_testRootDirectory, Array.Empty<string>());

        Assert.That(config.Root, Is.EqualTo(_testRootDirectory));
    }

    [Test]
    public void ToString_ReturnsRootDirectory()
    {
        var config = new DirectoriesConfig(_testRootDirectory, "Assets");

        var result = config.ToString();

        Assert.That(result, Is.EqualTo(_testRootDirectory));
    }
}
