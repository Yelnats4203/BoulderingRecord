using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class SendRepository(BoulderingRecordDbContext dbContext) : ISendRepository
{
    public Task<Send?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Sends.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<List<Send>> GetByUploaderIdAsync(
        Guid uploaderId,
        string? gymName,
        DateOnly? uploadedFrom,
        DateOnly? uploadedTo,
        int? minDifficulty,
        int? maxDifficulty,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Send> query = dbContext.Sends.Where(s => s.UploaderId == uploaderId);

        if (!string.IsNullOrWhiteSpace(gymName))
        {
            query = query.Where(s => s.GymName != null && s.GymName.Contains(gymName));
        }

        if (uploadedFrom is not null)
        {
            query = query.Where(s => s.UploadedAt >= uploadedFrom.Value);
        }

        if (uploadedTo is not null)
        {
            query = query.Where(s => s.UploadedAt <= uploadedTo.Value);
        }

        if (minDifficulty is not null)
        {
            query = query.Where(s => s.Difficulty != null && s.Difficulty >= minDifficulty.Value);
        }

        if (maxDifficulty is not null)
        {
            query = query.Where(s => s.Difficulty != null && s.Difficulty <= maxDifficulty.Value);
        }

        List<Send> sends = await query.ToListAsync(cancellationToken);
        return sends.OrderByDescending(s => s.UploadedAt).ToList();
    }

    public async Task AddAsync(Send send, CancellationToken cancellationToken = default)
        => await dbContext.Sends.AddAsync(send, cancellationToken);

    public Task DeleteAsync(Send send, CancellationToken cancellationToken = default)
    {
        dbContext.Sends.Remove(send);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
