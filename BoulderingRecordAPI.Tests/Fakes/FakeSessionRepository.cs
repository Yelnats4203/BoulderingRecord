using BoulderingRecordAPI.Repositories;
using Session = BoulderingRecordAPI.Entities.Session;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeSessionRepository(IEnumerable<Session>? seedSessions = null) : ISessionRepository
{
    private readonly List<Session> _sessions = seedSessions?.ToList() ?? [];

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_sessions.FirstOrDefault(s => s.Id == id));

    public Task<List<Session>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_sessions.Where(s => s.UserId == userId).OrderByDescending(s => s.Date).ToList());

    public Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }

    public void Remove(Session session) => _sessions.Remove(session);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
