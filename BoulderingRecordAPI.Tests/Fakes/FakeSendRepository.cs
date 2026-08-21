using BoulderingRecordAPI.Repositories;
using Send = BoulderingRecordAPI.Entities.Send;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeSendRepository(IEnumerable<Send>? seedSends = null) : ISendRepository
{
    private readonly List<Send> _sends = seedSends?.ToList() ?? [];

    public Task<Send?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_sends.FirstOrDefault(s => s.Id == id));

    public Task<List<Send>> GetByUploaderIdAsync(
        Guid uploaderId,
        string? gymName,
        DateOnly? climbAtFrom,
        DateOnly? climbAtTo,
        int? minDifficulty,
        int? maxDifficulty,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Send> query = _sends.Where(s => s.UploaderId == uploaderId);

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

        return Task.FromResult(query.OrderByDescending(s => s.ClimbAt).ToList());
    }

    public Task<int> CountByUploaderIdAndUploadedDateAsync(
        Guid uploaderId,
        DateOnly uploadedDate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_sends.Count(s => s.UploaderId == uploaderId && s.UploadedAt == uploadedDate));

    public Task AddAsync(Send send, CancellationToken cancellationToken = default)
    {
        _sends.Add(send);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Send send, CancellationToken cancellationToken = default)
    {
        _sends.RemoveAll(s => s.Id == send.Id);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
