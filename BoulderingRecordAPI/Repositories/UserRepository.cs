using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class UserRepository(BoulderingRecordDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByAccAsync(string acc, CancellationToken cancellationToken = default)
        => dbContext.Users.FirstOrDefaultAsync(u => u.Acc == acc, cancellationToken);

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => dbContext.Users.OrderBy(u => u.CreatedAt).ToListAsync(cancellationToken);

    public Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync(cancellationToken);

    public Task<List<User>> SearchByUsernameAsync(
        string keyword, Guid excludeUserId, bool excludeEditPermissionUsers, CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = dbContext.Users
            .Where(u => u.Id != excludeUserId && u.Username.Contains(keyword));

        if (excludeEditPermissionUsers)
        {
            query = query.Where(u => !u.HasEditPermission);
        }

        return query.OrderBy(u => u.Username).Take(20).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await dbContext.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
