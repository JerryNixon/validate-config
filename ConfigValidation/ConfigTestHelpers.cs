using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using System.Reflection;
using System.Resources;
using System.Text.Json;

using ValidateConsole.Models;

public static class ConfigTestHelpers
{
    /// <summary>
    /// Creates a <see cref="ConfigError"/> instance with the specified error number, line number, and optional message.
    /// </summary>
    /// <param name="Number">The error number.</param>
    /// <param name="LineNumber">The line number where the error occurred.</param>
    /// <param name="message">An optional error message.</param>
    /// <returns>A <see cref="ConfigError"/> instance.</returns>
    public static ConfigError CreateConfigurationError(string Number, int LineNumber = 0, string? message = null)
    {
        var rm = new ResourceManager("ConfigValidation.ConfigErrors", typeof(ConfigError).Assembly);
        return new ConfigError(Number, LineNumber, message ?? rm.GetString(Number) ?? throw new NullReferenceException($"Resource key not found: {Number}"));
    }

    /// <summary>
    /// Gets all methods in the specified object's type that are marked with the <see cref="ConfigTestAttribute"/>.
    /// </summary>
    /// <param name="o">The object whose type's methods to retrieve.</param>
    /// <returns>An enumerable collection of <see cref="MethodInfo"/> objects.</returns>
    public static IEnumerable<MethodInfo> GetTestMethods(this object o) => GetTestMethods(o.GetType());

    /// <summary>
    /// Gets all methods in the specified type that are marked with the <see cref="ConfigTestAttribute"/>.
    /// </summary>
    /// <param name="type">The type whose methods to retrieve.</param>
    /// <returns>An enumerable collection of <see cref="MethodInfo"/> objects.</returns>
    public static IEnumerable<MethodInfo> GetTestMethods(Type type)
    {
        return type
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ConfigTestAttribute), false).Any());
    }

    /// <summary>
    /// Validates a JSON string against a JSON schema.
    /// </summary>
    /// <param name="schemaPath">The path to the JSON schema file.</param>
    /// <param name="json">The JSON string to validate.</param>
    /// <param name="errors">An output parameter that receives any validation errors.</param>
    /// <returns><c>true</c> if the JSON string is valid according to the schema; otherwise, <c>false</c>.</returns>
    public static bool ValidateSchema(string schemaPath, string json, out IList<string> errors)
    {
        try
        {
            var schemaJson = File.ReadAllText(schemaPath);
            var schema = JSchema.Parse(schemaJson);
            var jsonDataObject = JObject.Parse(json);
            return jsonDataObject.IsValid(schema, out errors);
        }
        catch
        {
            errors = new List<string>();
            return false;
        }
    }

    /// <summary>
    /// Tokenizes a JSON string into a sequence of <see cref="ConfigProperty"/> objects.
    /// </summary>
    /// <param name="json">The JSON string to tokenize.</param>
    /// <returns>An enumerable collection of <see cref="ConfigProperty"/> objects.</returns>
    public static IEnumerable<ConfigProperty> Tokenize(string json)
    {
        var document = JsonDocument.Parse(json);
        var pathStack = new Stack<string>();
        var lineNumber = 1;

        foreach (var element in document.RootElement.EnumerateObject())
        {
            var propertyName = element.Name;
            var fullPath = BuildFullPath(pathStack, propertyName);

            if (element.Value.ValueKind == JsonValueKind.Object)
            {
                pathStack.Push(fullPath);
                foreach (var property in TokenizeNested(element.Value, pathStack, lineNumber))
                {
                    yield return property;
                }
                pathStack.Pop();
            }
            else
            {
                var value = element.Value.ToString();
                yield return new ConfigProperty(fullPath, value, lineNumber);
            }

            lineNumber += CountLineBreaks(element.Value.ToString());
        }
    }

    /// <summary>
    /// Tokenizes a JSON element and its nested elements into a sequence of <see cref="ConfigProperty"/> objects.
    /// </summary>
    /// <param name="element">The JSON element to tokenize.</param>
    /// <param name="pathStack">A stack of property paths used for building the full path.</param>
    /// <param name="lineNumber">The current line number within the JSON string.</param>
    /// <returns>An enumerable collection of <see cref="ConfigProperty"/> objects.</returns>
    private static IEnumerable<ConfigProperty> TokenizeNested(JsonElement element, Stack<string> pathStack, int lineNumber)
    {
        foreach (var innerElement in element.EnumerateObject())
        {
            var propertyName = innerElement.Name;
            var fullPath = BuildFullPath(pathStack, propertyName);

            if (innerElement.Value.ValueKind == JsonValueKind.Object)
            {
                pathStack.Push(fullPath);
                foreach (var property in TokenizeNested(innerElement.Value, pathStack, lineNumber))
                {
                    yield return property;
                }
                pathStack.Pop();
            }
            else
            {
                var value = innerElement.Value.ToString();
                yield return new ConfigProperty(fullPath, value, lineNumber);
            }

            lineNumber += CountLineBreaks(innerElement.Value.ToString());
        }
    }

    /// <summary>
    /// Builds the full property path by joining the elements in the <paramref name="pathStack"/> with the specified <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="pathStack">A stack of property names representing the current path.</param>
    /// <param name="propertyName">The name of the current property.</param>
    /// <returns>The full property path.</returns>
    private static string BuildFullPath(Stack<string> pathStack, string propertyName)
    {
        var fullPath = string.Join(".", pathStack.Reverse()) + (pathStack.Count > 0 ? "." : "") + propertyName;
        return fullPath;
    }

    /// <summary>
    /// Counts the number of line breaks (newline characters) in the given text.
    /// </summary>
    /// <param name="text">The text in which to count line breaks.</param>
    /// <returns>The number of line breaks in the text.</returns>
    private static int CountLineBreaks(string text)
    {
        var count = 0;
        var position = -1;

        while ((position = text.IndexOf(Environment.NewLine, position + 1)) != -1)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Checks if the provided string is valid JSON.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <returns><c>true</c> if the input string is valid JSON; otherwise, <c>false</c>.</returns>
    public static bool IsValidJson(string input)
    {
        try
        {
            JsonDocument.Parse(input);
        }
        catch (JsonException)
        {
            return false;
        }

        return true;
    }
}
