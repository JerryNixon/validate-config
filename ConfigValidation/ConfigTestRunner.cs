using ValidateConsole.Models;

using static ConfigTestHelpers;

public class ConfigTestRunner
{
    private readonly string _schemaPath;

    public ConfigTestRunner(string schemaPath)
    {
        _schemaPath = schemaPath;
    }

    public IEnumerable<ConfigError> Validate(string path)
    {
        if (!File.Exists(path))
        {
            yield return CreateConfigurationError("CV001");
            yield break;
        }

        var json = File.ReadAllText(path);

        if (!IsValidJson(json))
        {
            yield return CreateConfigurationError("CV002");
            yield break;
        }

        if (!ValidateSchema(_schemaPath, json, out var schemaErrors))
        {
            yield return CreateConfigurationError("VC003", 0, string.Join(Environment.NewLine, schemaErrors));
            yield break;
        }

        var configuration = Tokenize(json).ToArray();

        var tests = new ConfigTests();

        foreach (var test in tests.GetTestMethods().AsParallel())
        {
            if (test.Invoke(tests, new[] { configuration }) is ConfigError result && result is not null)
            {
                yield return result;
            }
        }
    }
}