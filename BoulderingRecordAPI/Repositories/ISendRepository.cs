using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Repositories;

public interface ISendRepository
{
    Task<Send?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Send>> GetByUploaderIdAsync(
        Guid uploaderId,
        string? gymName,
        DateOnly? uploadedFrom,
        DateOnly? uploadedTo,
        int? minDifficulty,
        int? maxDifficulty,
        CancellationToken cancellationToken = default);

    Task AddAsync(Send send, CancellationToken cancellationToken = default);

    Task DeleteAsync(Send send, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
