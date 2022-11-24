using System.Threading.Tasks.Dataflow;

var simulatedDb = new SimulatedDb();

var notificationProducer = new TransformManyBlock<string, NotificationBatch>(GenerateNotificationBatchesAsync, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
var notificationSender = new ActionBlock<NotificationBatch>(SendNotificationsAsync, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

notificationProducer.LinkTo(notificationSender, new DataflowLinkOptions { PropagateCompletion = true });

WriteTimedLine("Start processing topics");

var notificationTopics = new[] { "Gaming", "Furniture", "Smartphones", "Furniture" };
foreach (var topic in notificationTopics)
{
    await notificationProducer.SendAsync(topic);
}

notificationProducer.Complete();
await notificationSender.Completion;

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

async Task SendNotificationsAsync(NotificationBatch notificationBatch)
{
    await Task.Delay(TimeSpan.FromSeconds(1));

    WriteTimedLine($"Sent notification for topic {notificationBatch.Topic} to {notificationBatch.UserIds.Count} users");
}

void WriteTimedLine(string text) => Console.WriteLine($"[{DateTime.Now.ToString("T")}] {text}");

record NotificationBatch(string Topic, List<int> UserIds);
