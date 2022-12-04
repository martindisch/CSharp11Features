# My favorite C# 11/.NET 7 features

## Raw string literals

Luckily I don't have to do a lot of manual formatting of semi-structured data,
but whenever I do, especially if the text contains quotes or curly braces, it
gets ugly very quickly. Not anymore!

```csharp
var id = 25;

var meep = $"""
<person id="{id}">
    <name>John Doe</name>
    <address kind="home">Market Street 45</address>
</person>
""";
```

Not only does string interpolation still work (you can even include raw curly
braces without having to escape them by increasing the number of dollar signs
at the start), it also removes the newlines after the opening quote and the
same plus any whitespace before the closing quote so you have a decent chance
of ending up with what you intended.

## List pattern

This finally gives C# pattern matching capabilities on par with
[the best programming languages](https://adventures.michaelfbryan.com/posts/daily/slice-patterns/#checking-for-palindromes)
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

## Bonus: IAsyncEnumerable support in Dataflow TransformManyBlock

Admittedly, this is a rather niche thing that I'm pretty sure barely anybody
knows about. But those who do, have wanted it badly. If you haven't heard of
[TPL Dataflow](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library)
yet, I highly recommend reading up on it. It's a hidden gem of the .NET
Framework.

To demonstrate the new superpower that Dataflow has gained with .NET 7, let's
imagine an example application. It's a system that sends notifications with
news about certain topics to users that have subscribed to those topics. Our
pipeline therefore has two stages/blocks:

- A notification producer, which takes a topic for which we have news (a simple
  string with the topic name) and returns a `Notification`. This is merely a
  tuple containing the topic name and IDs of all users that are subscribed to
  it and should therefore receive a notification. It's modeled as
  `record Notification(string Topic, List<int> UserIds)`. This notification
  producer loads the IDs of all users that have subscribed to a certain topic
  from the database.
- A notification sender, which accepts such a `Notification` and notifies the
  users in it about the topic.

Our pipeline is then defined as

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

The `BoundedCapacity` is there to provide backpressure, guaranteeing that we
don't load users from the database faster than we can send notifications. This
prevents unnecessary load on the database and potentially high memory usage for
buffering all the accumulated notifications, in case loading them from the
database is faster than sending notifications.

This is fine and dandy, as long as `GenerateNotificationAsync` can load all
users for a topic at once to return a `Notification` and pass them on to the
next block. However, that is quite unlikely. You might have many hundreds of
thousands of users, meaning that you would most likely want to iterate over
your database in a paging fashion to produce batches of user IDs instead. Easy
enough! Why don't we rename `Notification` to
`record NotificationBatch(string Topic, List<int> UserIds)` and turn the first
block into a `TransformManyBlock`?

```csharp
var notificationProducer = new TransformManyBlock<string, NotificationBatch>(
    GenerateNotificationBatchesAsync,
    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
```

Like its `SelectMany` counterpart in LINQ, the function in this block now
accepts a single topic and produces many batches of notifications. The trouble
starts when thinking about how to implement this. If we still have to write an
async function that takes a single string and returns a list of
`NotificationBatch`, don't we have the same problem as before? That we have to
load all user IDs for the topic at once and then return them, this time in a
list of smaller `NotificationBatch` chunks? It may look this way, but remember
that the signature of the function we provide for the `TransformManyBlock` is
`Func<TInput, Task<IEnumerable<TOutput>>>`. Since we can return an enumerable,
couldn't we use an iterator method that does
`yield return new NotificationBatch(...)` after having loaded the next batch of
user IDs for the topic from the database? That would produce a nice, steady
stream of batches that would flow into the next block as they become available.

We can, but only if our database access is synchronous, as it's of course not
possible to await within an iterator method returning `IEnumerable`. And that's
exactly the problem described in
[this issue](https://github.com/dotnet/runtime/issues/30863)
that has been solved in .NET 7. It's now possible to to provide a
transformation `Func<TInput, IAsyncEnumerable<TOutput>>` to achieve this. Now
we can implement our async iterator method for the block:

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

It uses a paging mechanism on the database to load the next batch of user IDs
for the topic, providing a reference to the last entry of the previous batch so
it knows where to continue. If no more users are found it terminates, otherwise
it yields the current batch before continuing with the next one. As a result,
it will only begin loading the next batch after it has been able to yield the
current one, which it will only be able to do if the subsequent block
(notification sender) accepts the next input, which it in turn will only do
once it's ready to send another batch of notifications. In this way we have
perfect backpressure throughout the pipeline, where the slowest block is the
deciding factor for the rate at which items will be processed.

If we log whenever a `NotificationBatch` has been sent in our toy example,
configure it such that

- Loading the next batch of user IDs for a topic takes 1 second (batch size 5)
- Sending a `NotificationBatch` takes 2 seconds

and push the following topics into the first block

```csharp
var notificationTopics = new[] { "Gaming", "Furniture", "Smartphones", "Fashion" };
foreach (var topic in notificationTopics)
{
    await notificationProducer.SendAsync(topic);
}

notificationProducer.Complete();
await notificationSender.Completion;
```

we might see something like the following.

```console
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

It takes the first batch 3 seconds to pass through the pipeline, because cycle
time is 1 + 2 seconds. But after that we are able to fully process a batch
every 2 seconds, which is the time the bottleneck (sending notifications)
takes.

## License

Licensed under [MIT license](LICENSE).
