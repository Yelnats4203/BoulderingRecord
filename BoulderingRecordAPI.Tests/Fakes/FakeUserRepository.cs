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

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
