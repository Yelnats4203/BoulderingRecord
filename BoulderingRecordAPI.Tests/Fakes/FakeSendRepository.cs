using BoulderingRecordAPI.Repositories;
using Send = BoulderingRecordAPI.Entities.Send;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeSendRepository(IEnumerable<Send>? seedSends = null) : ISendRepository
{
    private readonly List<Send> _sends = seedSends?.ToList() ?? [];

    public Task<Send?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_sends.FirstOrDefault(s => s.Id == id));

    public Task<List<Send>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_sends.OrderByDescending(s => s.UploadedAt).ToList());

    public Task AddAsync(Send send, CancellationToken cancellationToken = default)
    {
        _sends.Add(send);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
