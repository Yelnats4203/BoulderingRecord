using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Repositories;

public interface IRecordRepository
{
    Task<Record?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Record>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Record record, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
