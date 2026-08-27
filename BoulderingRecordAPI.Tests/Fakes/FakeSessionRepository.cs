using BoulderingRecordAPI.Repositories;
using Session = BoulderingRecordAPI.Entities.Session;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeSessionRepository(IEnumerable<Session>? seedSessions = null) : ISessionRepository
{
    private readonly List<Session> _sessions = seedSessions?.ToList() ?? [];

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_sessions.FirstOrDefault(s => s.Id == id));

    public Task<List<Session>> GetAllByUserIdAsync(
        Guid userId, DateOnly? dateFrom = null, DateOnly? dateTo = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Session> query = _sessions.Where(s => s.UserId == userId);

        if (dateFrom is not null)
        {
            query = query.Where(s => s.Date >= dateFrom.Value);
        }

        if (dateTo is not null)
        {
            query = query.Where(s => s.Date <= dateTo.Value);
        }

        return Task.FromResult(query.OrderByDescending(s => s.Date).ToList());
    }

    public Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }

    public void Remove(Session session) => _sessions.Remove(session);

    public Func<Task>? SaveChangesOverride { get; set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesOverride?.Invoke() ?? Task.CompletedTask;
}
