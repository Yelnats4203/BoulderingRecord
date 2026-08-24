using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Repositories;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeUserRepository(IEnumerable<User> seedUsers) : IUserRepository
{
    private readonly List<User> _users = seedUsers.ToList();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByAccAsync(string acc, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(u => u.Acc == acc));

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_users.OrderBy(u => u.CreatedAt).ToList());

    public Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.Where(u => ids.Contains(u.Id)).ToList());

    public Task<List<User>> SearchByUsernameAsync(
        string keyword, Guid excludeUserId, bool excludeEditPermissionUsers, CancellationToken cancellationToken = default)
    {
        IEnumerable<User> query = _users.Where(u => u.Id != excludeUserId && u.Username.Contains(keyword));

        if (excludeEditPermissionUsers)
        {
            query = query.Where(u => !u.HasEditPermission);
        }

        return Task.FromResult(query.OrderBy(u => u.Username).Take(20).ToList());
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
