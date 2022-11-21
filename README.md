# My favorite C# 11/.NET 7 features

## Raw string literals

How many times have I tried to achieve something like this (not to hack
together dangerous SQL statements of course), only to end up with an ugly
"+"-concatenated mess with terrible indentation?

```csharp
var id = 25;

var fantasticSqlInjection = $"""
-- I can use "quotes" as much as I like!
SELECT * FROM schema.table
WHERE Id = {id}
""";
```

Not only does string interpolation still work (you can even include raw curly
braces without having to escape them by increasing the number of dollar signs
at the start), it also removes the newlines after the opening quote and the
same plus any whitespace before the closing quote so you have a decent chance
of ending up with what you intended.

## List pattern

This finally gives C# pattern matching capabilities on par with [the best
programming languages](https://adventures.michaelfbryan.com/posts/daily/slice-patterns/#checking-for-palindromes)
out there. You can start writing some truly beautiful stuff.

```csharp
bool IsPalindrome(char[] characters) => characters switch
{
    [var first, .. var middle, var last] => first == last && IsPalindrome(middle),
    [] or [_] => true,
};
```

## Generic parse/static interface members

Thanks to some fancy type system features, the long-standing convention of the
static `Parse` factory method can be formalized through the `IParsable<TSelf>`
interface. It's now possible to have interfaces with static members, which
opens way more cool possibilities than just this.

Given an implementation on your type like this one

```csharp
record PoorCulture(string Language, string Country) : IParsable<PoorCulture>
{
    public static PoorCulture Parse(string s, IFormatProvider? provider)
    {
        var parts = s.Split('-');
        if (parts.Length != 2)
        {
            throw new FormatException("Oh noes");
        }

        return new(parts[0], parts[1]);
    }

    // TryParse omitted for brevity
}
```

you can use this now standardized implementation of `Parse` in a function like

```csharp
static T GenericParse<T>(string input) where T : IParsable<T>
{
    return T.Parse(input, null);
}
```

which accepts all other parsable types like integers as well.

## Required modifier

The object initializer syntax is pretty sweet. But for a long time, using it
has been extremely unsafe. Consider the following:

```csharp
record InsanePerson
{
    public int Age { get; init; }
    public string FirstName { get; init; } = null!;
    public string? MiddleName { get; init; }
}

// We just made a mistake. Is it obvious? No.
var invalidPerson = new InsanePerson { MiddleName = "None" };
```

With the type receiving the dreaded default constructor, even the compiler
notices that `FirstName` for example could be left uninitialized, so in our
infinite wisdom we decide to shut it up with the "trust me"-operator. This is
completely our fault, but a pattern we see frequently. Now for the value type
`Age` the language shows us no such kindness, as it will silently
auto-initialize to the default value of 0 if we forget to set it. Just great.

The solution is to create constructors on all types to deny the dumb default
constructor. But constructors are so verbose! Luckily, C# 9 graced us with its
extremely concise positional record syntax, allowing us to define a type and
have its full constructor implemented at the same time on a single line.

```csharp
record PositionalPerson(int Age, string FirstName, string? MiddleName = null);

// Obviously fails to compile, as it should
var invalidPerson = new PositionalPerson("None");
```

This is nice and all, but what if you can't or won't use records and are stuck
with classes? C# 11 comes to the rescue with `required` members.

```csharp
class RequiredPerson
{
    public required int Age { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
}

// This time the compiler will stop us from shooting ourselves in the foot
var invalidPerson = new RequiredPerson { MiddleName = "None" };
```

Beautiful!

## License

Licensed under either of

- [Apache License, Version 2.0](LICENSE-APACHE)
- [MIT license](LICENSE-MIT)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, as defined in the Apache-2.0 license, shall
be dual licensed as above, without any additional terms or conditions.
