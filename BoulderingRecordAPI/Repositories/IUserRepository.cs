using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByAccAsync(string acc, CancellationToken cancellationToken = default);

    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<List<User>> SearchByUsernameAsync(
        string keyword, Guid excludeUserId, bool excludeEditPermissionUsers, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
