using System.Threading.Tasks.Dataflow;

var multiplier = new TransformManyBlock<int, TwoNumbers>(MultiplierAsync, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
var consumer = new ActionBlock<TwoNumbers>(Printer, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

multiplier.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

foreach (var n in Enumerable.Range(1, 5))
{
    await multiplier.SendAsync(n);
}

multiplier.Complete();

await consumer.Completion;

async IAsyncEnumerable<TwoNumbers> MultiplierAsync(int number)
{
    foreach (var n in Enumerable.Range(1, 5))
    {
        await Task.Delay(50);
        yield return new(number, n);
    }
}

void Printer(TwoNumbers numbers)
{
    Console.WriteLine(numbers);
}

record TwoNumbers(int First, int Second);
