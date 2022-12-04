---
# https://sli.dev/custom/highlighters.html
highlighter: shiki
---

# My favorite C# 11/.NET 7 features

---

# Testing some code

```csharp
bool IsPalindrome(char[] characters) => characters switch
{
    [var first, .. var middle, var last] => first == last && IsPalindrome(middle),
    [] or [_] => true,
};
```

Seems cool.
