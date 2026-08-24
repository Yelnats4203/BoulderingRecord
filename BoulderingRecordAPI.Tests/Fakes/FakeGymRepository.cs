using BoulderingRecordAPI.Repositories;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeGymRepository(IEnumerable<string>? seedGymNames = null) : IGymRepository
{
    private readonly List<string> _gymNames = seedGymNames?.ToList() ?? [];

    public Task<List<string>> GetDistinctGymNamesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_gymNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name, StringComparer.CurrentCulture)
            .ToList());
}
