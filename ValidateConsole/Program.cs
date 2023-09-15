using System.Collections;
using System.Reflection;
using System.Resources;
using System.Xml.Linq;

var schemaPath = "Samples/schema.json";

var jsonPath = "Samples/sample.json";

Console.WriteLine($"Errors to check for:");
Console.ForegroundColor = ConsoleColor.Yellow;
var settings = ReadEmbeddedResx();
foreach (var item in settings)
{
    Console.WriteLine($"{item.Name} - {item.Message}");
}

Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Using this JSON content:");
Console.ForegroundColor = ConsoleColor.Green;
var json = File.ReadAllText(jsonPath);
foreach (var line in json.Split(Environment.NewLine))
{
    Console.WriteLine(line);
}

Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Errors Found...");

var errors = new ConfigTestRunner(schemaPath).Validate(jsonPath);

Console.ForegroundColor = ConsoleColor.Red;

foreach (var error in errors)
{
    Console.WriteLine($"Error:{error.Number} - Line:{error.LineNumber} - {error.Message}");
}

Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Done.");
Console.Read();

IEnumerable<(string Name, string Message)> ReadEmbeddedResx()
{
    // Load the assembly where the .resx is embedded
    var assembly = typeof(ConfigTestRunner).Assembly;

    // Create a ResourceManager for the .resx file
    var resourceManager = new ResourceManager($"{assembly.GetName().Name}.ConfigErrors", assembly);

    // Get the names of all embedded resources in the assembly
    var resourceNames = assembly.GetManifestResourceNames();

    foreach (var resourceName in resourceNames)
    {
        if (resourceName.EndsWith(".ConfigErrors.resources"))
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var resourceReader = new ResourceReader(stream);

                    foreach (DictionaryEntry entry in resourceReader)
                    {
                        var name = entry.Key.ToString();
                        var message = entry.Value.ToString();

                        yield return (name, message);
                    }
                }
            }
        }
    }
}