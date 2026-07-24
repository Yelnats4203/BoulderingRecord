using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class RecordRepository(BoulderingRecordDbContext dbContext) : IRecordRepository
{
    public Task<Record?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Records.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<List<Record>> GetAllAsync(CancellationToken cancellationToken = default)
        => dbContext.Records.OrderByDescending(r => r.UploadedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Record record, CancellationToken cancellationToken = default)
        => await dbContext.Records.AddAsync(record, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
