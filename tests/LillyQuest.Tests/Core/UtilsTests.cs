using LillyQuest.Core.Utils;

namespace LillyQuest.Tests.Core;

/// <summary>
/// Tests for StringUtils case conversion methods
/// </summary>
public class StringUtilsTests
{
#region DotCase Tests

    [Test, TestCase("HelloWorld", "hello.world"), TestCase("API_RESPONSE", "api.response"), TestCase("userId", "user.id")]
    public void ToDotCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToDotCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

#endregion

#region PathCase Tests

    [Test, TestCase("HelloWorld", "hello/world"), TestCase("API_RESPONSE", "api/response"), TestCase("userId", "user/id")]
    public void ToPathCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToPathCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

#endregion

#region UpperSnakeCase Tests

    [Test, TestCase("HelloWorld", "HELLO_WORLD"), TestCase("apiResponse", "API_RESPONSE"), TestCase("user-id", "USER_ID")]
    public void ToUpperSnakeCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToUpperSnakeCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

#endregion

#region CamelCase Tests

    [Test, TestCase("HelloWorld", "helloWorld"), TestCase("API_RESPONSE", "apiResponse"), TestCase("user-id", "userId"),
     TestCase("user_id", "userId"), TestCase("helloWorld", "helloWorld"), TestCase("HELLO", "hello")]
    public void ToCamelCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToCamelCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToCamelCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToCamelCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToCamelCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToCamelCase(null);

        Assert.That(result, Is.Empty);
    }

    [Test, TestCase("a"), TestCase("A")]
    public void ToCamelCase_WithSingleCharacter_ReturnsLowercased(string input)
    {
        var result = StringUtils.ToCamelCase(input);

        Assert.That(result, Is.EqualTo(input.ToLowerInvariant()));
    }

#endregion

#region PascalCase Tests

    [Test, TestCase("hello_world", "HelloWorld"), TestCase("api-response", "ApiResponse"), TestCase("userId", "UserId"),
     TestCase("user_id", "UserId"), TestCase("HelloWorld", "HelloWorld")]
    public void ToPascalCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToPascalCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToPascalCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToPascalCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToPascalCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToPascalCase(null);

        Assert.That(result, Is.Empty);
    }

    [Test, TestCase("a"), TestCase("z")]
    public void ToPascalCase_WithSingleCharacter_ReturnsUppercased(string input)
    {
        var result = StringUtils.ToPascalCase(input);

        Assert.That(result, Is.EqualTo(input.ToUpperInvariant()));
    }

#endregion

#region SnakeCase Tests

    [Test, TestCase("HelloWorld", "hello_world"), TestCase("APIResponse", "api_response"), TestCase("userId", "user_id"),
     TestCase("API_RESPONSE", "api_response"), TestCase("hello-world", "hello_world")]
    public void ToSnakeCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToSnakeCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToSnakeCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToSnakeCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToSnakeCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToSnakeCase(null);

        Assert.That(result, Is.Empty);
    }

#endregion

#region KebabCase Tests

    [Test, TestCase("HelloWorld", "hello-world"), TestCase("API_RESPONSE", "api-response"), TestCase("userId", "user-id"),
     TestCase("hello_world", "hello-world")]
    public void ToKebabCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToKebabCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToKebabCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToKebabCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToKebabCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToKebabCase(null);

        Assert.That(result, Is.Empty);
    }

#endregion

#region TitleCase Tests

    [Test, TestCase("hello_world", "Hello World"), TestCase("API_RESPONSE", "Api Response"), TestCase("user-id", "User Id"),
     TestCase("userId", "User Id")]
    public void ToTitleCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToTitleCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToTitleCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToTitleCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToTitleCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToTitleCase(null);

        Assert.That(result, Is.Empty);
    }

#endregion

#region SentenceCase Tests

    [Test, TestCase("hello world", "Hello world"), TestCase("API_RESPONSE", "Api response"), TestCase("userId", "Userid")]
    public void ToSentenceCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToSentenceCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToSentenceCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToSentenceCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToSentenceCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToSentenceCase(null);

        Assert.That(result, Is.Empty);
    }

#endregion

#region TrainCase Tests

    [Test, TestCase("hello_world", "Hello-World"), TestCase("apiResponse", "Api-Response"), TestCase("user_id", "User-Id")]
    public void ToTrainCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        var result = StringUtils.ToTrainCase(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToTrainCase_WithEmpty_ReturnsEmpty()
    {
        var result = StringUtils.ToTrainCase("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToTrainCase_WithNull_ReturnsEmpty()
    {
        var result = StringUtils.ToTrainCase(null);

        Assert.That(result, Is.Empty);
    }

#endregion

#region Edge Cases

    [Test]
    public void CaseConversions_WithConsecutiveCapitals_HandleCorrectly()
    {
        // "APIResponse" should split as "API" + "Response"
        var snakeCase = StringUtils.ToSnakeCase("APIResponse");
        var camelCase = StringUtils.ToCamelCase("APIResponse");

        Assert.That(snakeCase, Is.EqualTo("api_response"));
        Assert.That(camelCase, Is.EqualTo("apiResponse"));
    }

    [Test]
    public void CaseConversions_WithSpaces_HandleCorrectly()
    {
        var snakeCase = StringUtils.ToSnakeCase("hello world");
        var camelCase = StringUtils.ToCamelCase("hello world");

        Assert.That(snakeCase, Is.EqualTo("hello_world"));
        Assert.That(camelCase, Is.EqualTo("helloWorld"));
    }

    [Test]
    public void CaseConversions_WithMixedDelimiters_HandleCorrectly()
    {
        var input = "hello-world_test case";

        var snake = StringUtils.ToSnakeCase(input);
        var camel = StringUtils.ToCamelCase(input);
        var pascal = StringUtils.ToPascalCase(input);

        Assert.That(snake, Does.Contain("_"));
        Assert.That(camel, Does.Not.Contain("_"));
        Assert.That(pascal, Does.Not.Contain("_"));
    }

    [Test]
    public void CaseConversions_WithNumbers_HandleCorrectly()
    {
        var snakeCase = StringUtils.ToSnakeCase("test123Case");

        // Numbers should be handled (typically attached to adjacent words)
        Assert.That(snakeCase, Does.Contain("test"));
        Assert.That(snakeCase, Does.Contain("case"));
    }

    [Test]
    public void Roundtrip_CamelToSnakeAndBack_ConservesInput()
    {
        var original = "userId";
        var toSnake = StringUtils.ToSnakeCase(original);
        var backToCamel = StringUtils.ToCamelCase(toSnake);

        Assert.That(toSnake, Is.EqualTo("user_id"));
        Assert.That(backToCamel, Is.EqualTo("userId"));
    }

#endregion
}
