using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class SessionRepository(BoulderingRecordDbContext dbContext) : ISessionRepository
{
    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<List<Session>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        List<Session> sessions = await dbContext.Sessions.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        return sessions.OrderByDescending(s => s.Date).ToList();
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        => await dbContext.Sessions.AddAsync(session, cancellationToken);

    public void Remove(Session session)
        => dbContext.Sessions.Remove(session);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
