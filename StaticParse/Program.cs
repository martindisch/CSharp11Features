var original = new PoorCulture("de", "CH");
var stringRepresentation = original.ToString();
var parsed = PoorCulture.Parse(stringRepresentation, null);

Console.WriteLine($"Original: {original}, Parsed: {parsed}");

var genericallyParsedInt = Util.GenericParse<int>("5");
var genericallyParsedPoorCulture = Util.GenericParse<PoorCulture>("de-CH");

Console.WriteLine($"Int: {genericallyParsedInt}, PoorCulture: {genericallyParsedPoorCulture}");
