using System.Threading.Tasks.Dataflow;

var multiplier = new TransformManyBlock<int, TwoNumbers>(Multiplier, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
var consumer = new ActionBlock<TwoNumbers>(Printer, new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

multiplier.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

foreach (var n in Enumerable.Range(1, 5))
{
    await multiplier.SendAsync(n);
}

multiplier.Complete();

await consumer.Completion;

IEnumerable<TwoNumbers> Multiplier(int number)
{
    foreach (var n in Enumerable.Range(1, 5))
    {
        yield return new(number, n);
    }
}

void Printer(TwoNumbers numbers)
{
    Console.WriteLine(numbers);
}

record TwoNumbers(int First, int Second);
