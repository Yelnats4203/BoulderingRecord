using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Repositories;

public interface ISendRepository
{
    Task<Send?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Send>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Send send, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
