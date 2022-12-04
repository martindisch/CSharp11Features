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

---
layout: section
---

# Required modifier

---

This was pretty bad.

```csharp
class InsanePerson
{
    public int Age { get; init; }
    public string FirstName { get; init; } = null!;
    public string? MiddleName { get; init; }
}

// We just made a mistake. Is it obvious? No.
var invalidPerson = new InsanePerson { MiddleName = "None" };
```

Constructors everywhere? Much boilerplate!

```csharp
record PositionalPerson(int Age, string FirstName, string? MiddleName = null);

// Obviously fails to compile, as it should
var invalidPerson = new PositionalPerson("None");
```

---

Behold `required`!

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

---
layout: section
---

# Bonus

## IAsyncEnumerable support in Dataflow TransformManyBlock

---

## What's this DataFlow thing again?

- Part of .NET Framework
- Allows building pipelines of multiple blocks that process and transform data
- Makes adding concurrency a breeze

## Example

A system that sends notifications with news about certain topics to users that
have subscribed to those topics. Our pipeline therefore has two stages/blocks.

1. A notification producer, which takes a topic for which we have news and
  returns a `Notification` containing the topic name and IDs of all users that
  are subscribed to it.
2. A notification sender, which accepts such a `Notification` and notifies the
  users in it about the topic.

```csharp
record Notification(string Topic, List<int> UserIds)
```

---

## Initial pipeline definition

```csharp
var notificationProducer = new TransformBlock<string, Notification>(
    GenerateNotificationAsync,
    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
var notificationSender = new ActionBlock<Notification>(
    SendNotificationsAsync,
    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

notificationProducer.LinkTo(
    notificationSender,
    new DataflowLinkOptions { PropagateCompletion = true });
```

BoundedCapacity serves as backpressure
- Great for preventing unnecessary load
- As well as memory usage

This works well as long as we can load _all_ users for a topic at once.
But what if that's not the case?

---

## Switching to batches

Turn the notification into

```csharp
record NotificationBatch(string Topic, List<int> UserIds)
```

and the first block into

```csharp
var notificationProducer = new TransformManyBlock<string, NotificationBatch>(
    GenerateNotificationBatchesAsync,
    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
```

- Now `GenerateNotificationBatchesAsync` returns a bunch of `NotificationBatch`
  for each topic
- But that doesn't help if it still has to return the full list of all batches
  in one go
- Since its signature is `Func<TInput, Task<IEnumerable<TOutput>>>`, we could
  use an iterator method that yields one batch after the other
- Except we can't. Or couldn't 😏

---

```csharp
async IAsyncEnumerable<NotificationBatch> GenerateNotificationBatchesAsync(string topic)
{
    int? after = null;

    while (true)
    {
        var userIds = await simulatedDb.GetNextSubscribedUsersBatchAsync(topic, after);
        if (userIds.Count == 0)
        {
            yield break;
        }

        after = userIds.Last();
        yield return new(topic, userIds);
    }
}
```

```csharp
var notificationTopics = new[] { "Gaming", "Furniture", "Smartphones", "Fashion" };
foreach (var topic in notificationTopics)
{
    await notificationProducer.SendAsync(topic);
}

notificationProducer.Complete();
await notificationSender.Completion;
```

---

```
$ dotnet run --project AsyncEnumerableDataflow
[1:38:12PM] Start processing topics
[1:38:15PM] Sent notification for topic Gaming to 5 users
[1:38:17PM] Sent notification for topic Gaming to 5 users
[1:38:19PM] Sent notification for topic Gaming to 5 users
[1:38:21PM] Sent notification for topic Furniture to 5 users
[1:38:23PM] Sent notification for topic Furniture to 5 users
[1:38:25PM] Sent notification for topic Furniture to 5 users
[1:38:27PM] Sent notification for topic Furniture to 5 users
[1:38:29PM] Sent notification for topic Furniture to 5 users
[1:38:31PM] Sent notification for topic Furniture to 5 users
[1:38:33PM] Sent notification for topic Furniture to 5 users
[1:38:35PM] Sent notification for topic Smartphones to 5 users
[1:38:37PM] Sent notification for topic Smartphones to 5 users
[1:38:39PM] Sent notification for topic Fashion to 5 users
[1:38:41PM] Sent notification for topic Fashion to 5 users
[1:38:43PM] Sent notification for topic Fashion to 5 users
```