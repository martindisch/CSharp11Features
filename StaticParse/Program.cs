var original = new PoorCulture("de", "CH");
var stringRepresentation = original.ToString();
var parsed = PoorCulture.Parse(stringRepresentation, null);

Console.WriteLine($"Original: {original}, Parsed: {parsed}");

static T GenericParse<T>(string input) where T : IParsable<T>
{
    return T.Parse(input, null);
}

var genericallyParsedInt = GenericParse<int>("5");
var genericallyParsedPoorCulture = GenericParse<PoorCulture>("de-CH");

Console.WriteLine($"Int: {genericallyParsedInt}, PoorCulture: {genericallyParsedPoorCulture}");
