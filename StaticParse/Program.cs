var original = new PoorCulture("de", "CH");
var stringRepresentation = original.ToString();
var parsed = PoorCulture.Parse(stringRepresentation, null);

Console.WriteLine($"Original: {original}, Parsed: {parsed}");
