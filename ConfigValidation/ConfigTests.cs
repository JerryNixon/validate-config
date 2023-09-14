using ValidateConsole.Models;

using static ConfigTestHelpers;

public class ConfigTests
{
    [ConfigTest]
    private ConfigError? ValidateConfig004_LocationWashington(ConfigProperty[] configuration)
    {
        var propertyName = "location";
        var property = configuration.FirstOrDefault(p => p.Path == propertyName);
        return property?.Value?.ToLower() == "washington"
            ? null // Return null when the condition is met
            : CreateConfigurationError("CV004", property?.LineNumber ?? 0);
    }

    [ConfigTest]
    private ConfigError? ValidateConfig005_NameFirstLength(ConfigProperty[] configuration)
    {
        var propertyName = "name.first";
        var property = configuration.FirstOrDefault(p => p.Path == propertyName);
        return property?.Value?.Length >= 5
            ? null // Return null when the condition is met
            : CreateConfigurationError("CV005", property?.LineNumber ?? 0);
    }

    [ConfigTest]
    private ConfigError? ValidateConfig006_NameLastLength(ConfigProperty[] configuration)
    {
        var propertyName = "name.last";
        var property = configuration.FirstOrDefault(p => p.Path == propertyName);
        return property?.Value?.Length >= 5
            ? null // Return null when the condition is met
            : CreateConfigurationError("CV006", property?.LineNumber ?? 0);
    }
}
