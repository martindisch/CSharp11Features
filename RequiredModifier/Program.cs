using System.Text.Json;

var invalidPerson = new InsanePerson { MiddleName = "None" };
Console.WriteLine(invalidPerson);

var positionalPerson = new PositionalPerson(25, "John");
Console.WriteLine(positionalPerson);

var requiredPerson = new RequiredPerson { FirstName = "John", Age = 25 };
// Serializing since as a class it doesn't get a nice ToString implementation for free
Console.WriteLine(JsonSerializer.Serialize(requiredPerson));
