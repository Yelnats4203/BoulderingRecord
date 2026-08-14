using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class SessionRepository(BoulderingRecordDbContext dbContext) : ISessionRepository
{
    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<List<Session>> GetAllByUserIdAsync(
        Guid userId, DateOnly? dateFrom = null, DateOnly? dateTo = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Session> query = dbContext.Sessions.Where(s => s.UserId == userId);

        if (dateFrom is not null)
        {
            query = query.Where(s => s.Date >= dateFrom.Value);
        }

        if (dateTo is not null)
        {
            query = query.Where(s => s.Date <= dateTo.Value);
        }

        List<Session> sessions = await query.ToListAsync(cancellationToken);
        return sessions.OrderByDescending(s => s.Date).ToList();
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        => await dbContext.Sessions.AddAsync(session, cancellationToken);

    public void Remove(Session session)
        => dbContext.Sessions.Remove(session);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
