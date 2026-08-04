using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class SendRepository(BoulderingRecordDbContext dbContext) : ISendRepository
{
    public Task<Send?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Sends.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<List<Send>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Send> sends = await dbContext.Sends.ToListAsync(cancellationToken);
        return sends.OrderByDescending(s => s.UploadedAt).ToList();
    }

    public async Task AddAsync(Send send, CancellationToken cancellationToken = default)
        => await dbContext.Sends.AddAsync(send, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
