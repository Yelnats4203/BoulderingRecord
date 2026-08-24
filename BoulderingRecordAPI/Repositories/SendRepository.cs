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
        DateOnly? climbAtFrom,
        DateOnly? climbAtTo,
        int? minDifficulty,
        int? maxDifficulty,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Send> query = dbContext.Sends.Where(s => s.UploaderId == uploaderId);

        if (!string.IsNullOrWhiteSpace(gymName))
        {
            query = query.Where(s => s.GymName != null && s.GymName.Contains(gymName));
        }

        if (climbAtFrom is not null)
        {
            query = query.Where(s => s.ClimbAt >= climbAtFrom.Value);
        }

        if (climbAtTo is not null)
        {
            query = query.Where(s => s.ClimbAt <= climbAtTo.Value);
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
        return sends.OrderByDescending(s => s.ClimbAt).ToList();
    }

    public Task<int> CountByUploaderIdAndUploadedDateAsync(
        Guid uploaderId,
        DateOnly uploadedDate,
        CancellationToken cancellationToken = default)
        => dbContext.Sends.CountAsync(s => s.UploaderId == uploaderId && s.UploadedAt == uploadedDate, cancellationToken);

    public Task<List<Send>> GetPublicByUploaderIdAsync(Guid uploaderId, CancellationToken cancellationToken = default)
        => dbContext.Sends
            .Where(s => s.UploaderId == uploaderId && s.Visibility == SendVisibility.Public)
            .OrderByDescending(s => s.ClimbAt)
            .ToListAsync(cancellationToken);

    public Task<List<Send>> GetRecentPublicByUploaderIdsAsync(
        IReadOnlyCollection<Guid> uploaderIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (uploaderIds.Count == 0)
        {
            return Task.FromResult(new List<Send>());
        }

        return dbContext.Sends
            .Where(s => uploaderIds.Contains(s.UploaderId) && s.Visibility == SendVisibility.Public)
            .OrderByDescending(s => s.UploadedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
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
