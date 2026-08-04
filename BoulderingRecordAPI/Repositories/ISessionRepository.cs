using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Session>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Session session, CancellationToken cancellationToken = default);

    void Remove(Session session);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
