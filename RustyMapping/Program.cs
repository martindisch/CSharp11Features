var person = new Person("John", new() { "Andrea", "Frank", "Suzie" });
var personResponse = PersonResponse.From(person);

Console.WriteLine(person);
Console.WriteLine(personResponse);
