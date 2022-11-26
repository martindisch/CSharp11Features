class SimulatedDb
{
    public async Task<List<int>> GetNextSubscribedUsersBatchAsync(string topic, int? after)
    {
        // Since we're simulating, make sure not to terminate on first
        // iteration and after that decide randomly (70% chance)
        var hasMore = after is null || Random.Shared.Next(10) < 7;
        if (!hasMore)
        {
            return new();
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        return Enumerable
            .Repeat(1, 5)
            .Select(_ => Random.Shared.Next(1, 10_000))
            .ToList();
    }
}
