---
# https://sli.dev/custom/highlighters.html
highlighter: shiki
---

# The best C# 11/.NET 7 features

## A biased selection

---
layout: section
---

# Raw string literals

---

## Remember these?

```csharp
string multiline_text = @"this is a multiline text
this is " + a1 + @"
this is " + a2 + @"
this is " + a3 + @"";
```

```csharp
var builder = new StringBuilder()
    .AppendLine("this is a multiline text")
    .AppendLine($"this is {a1}")
    .AppendLine($"this is {a2}")
    .AppendLine($"this is {a3}");
```

```csharp
string pattern = @"this is a multiline text
this is {0}
this is {1}
this is {2}";
string result = string.Format(pattern, a1, a2, a3);
```

```csharp
string pattern = $@"this is a multiline text
this is {a1}
this is {a2}
this is {a3}";
```

---

## This is better

```csharp
var id = 25;

var meep = $"""
<person id="{id}">
    <name>John Doe</name>
    <address kind="home">Market Street 45</address>
</person>
""";
```

- Works with raw curly braces without having to escape them
- Removes newlines after opening and same plus whitespace before closing quote

---
layout: section
---

# List pattern

---

## Almost 🦀

```csharp
bool IsPalindrome(char[] characters) => characters switch
{
    [var first, .. var middle, var last] => first == last && IsPalindrome(middle),
    [] or [_] => true,
};
```

---
layout: section
---

# Generic parse<br>/<br>static interface members

---

# Implement interface specifying static members

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

---

# Use it

## Specify interface as type bound

```csharp
static T GenericParse<T>(string input) where T : IParsable<T>
{
    return T.Parse(input, null);
}
```

## Select implementation with generic type parameter

```csharp
var genericallyParsedInt = GenericParse<int>("5");
var genericallyParsedPoorCulture = GenericParse<PoorCulture>("de-CH");
```