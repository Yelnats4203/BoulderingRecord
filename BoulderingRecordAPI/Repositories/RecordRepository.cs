using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class RecordRepository(BoulderingRecordDbContext dbContext) : IRecordRepository
{
    public Task<Record?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Records.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<List<Record>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Record> records = await dbContext.Records.ToListAsync(cancellationToken);
        return records.OrderByDescending(r => r.UploadedAt).ToList();
    }

    public async Task AddAsync(Record record, CancellationToken cancellationToken = default)
        => await dbContext.Records.AddAsync(record, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
