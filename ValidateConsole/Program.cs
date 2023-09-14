var schemaPath = "Samples/schema.json";

var jsonPath = "Samples/sample.json";

var errors = new ConfigTestRunner(schemaPath).Validate(jsonPath);

Console.ForegroundColor = ConsoleColor.Red;

foreach (var error in errors)
{
    Console.WriteLine($"Error:{error.Number} - {error.Message} - Line:{error.LineNumber}");
}

Console.ResetColor();

Console.WriteLine("Validate complete.");
