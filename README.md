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

## Generic parse/static interface members and generics

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

## License

Licensed under either of

- [Apache License, Version 2.0](LICENSE-APACHE)
- [MIT license](LICENSE-MIT)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, as defined in the Apache-2.0 license, shall
be dual licensed as above, without any additional terms or conditions.
