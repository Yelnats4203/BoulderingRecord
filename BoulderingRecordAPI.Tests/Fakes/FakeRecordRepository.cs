using BoulderingRecordAPI.Repositories;
using Record = BoulderingRecordAPI.Entities.Record;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeRecordRepository(IEnumerable<Record>? seedRecords = null) : IRecordRepository
{
    private readonly List<Record> _records = seedRecords?.ToList() ?? [];

    public Task<Record?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_records.FirstOrDefault(r => r.Id == id));

    public Task<List<Record>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_records.OrderByDescending(r => r.UploadedAt).ToList());

    public Task AddAsync(Record record, CancellationToken cancellationToken = default)
    {
        _records.Add(record);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
