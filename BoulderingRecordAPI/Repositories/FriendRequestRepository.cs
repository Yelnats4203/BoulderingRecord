using BoulderingRecordAPI.Data;
using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class FriendRequestRepository(BoulderingRecordDbContext dbContext) : IFriendRequestRepository
{
    public Task<FriendRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.FriendRequests.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<FriendRequest?> GetBetweenUsersAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default)
        => dbContext.FriendRequests.FirstOrDefaultAsync(
            f => (f.RequesterId == userAId && f.AddresseeId == userBId) || (f.RequesterId == userBId && f.AddresseeId == userAId),
            cancellationToken);

    public Task<List<FriendRequest>> GetPendingReceivedAsync(Guid addresseeId, CancellationToken cancellationToken = default)
        => dbContext.FriendRequests
            .Where(f => f.AddresseeId == addresseeId && f.Status == FriendRequestStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<FriendRequest>> GetAcceptedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => dbContext.FriendRequests
            .Where(f => f.Status == FriendRequestStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<FriendRequest>> GetRelationsForUsersAsync(
        Guid currentUserId,
        IReadOnlyCollection<Guid> otherUserIds,
        CancellationToken cancellationToken = default)
        => dbContext.FriendRequests
            .Where(f =>
                (f.RequesterId == currentUserId && otherUserIds.Contains(f.AddresseeId)) ||
                (f.AddresseeId == currentUserId && otherUserIds.Contains(f.RequesterId)))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FriendRequest friendRequest, CancellationToken cancellationToken = default)
        => await dbContext.FriendRequests.AddAsync(friendRequest, cancellationToken);

    public Task DeleteAsync(FriendRequest friendRequest, CancellationToken cancellationToken = default)
    {
        dbContext.FriendRequests.Remove(friendRequest);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
