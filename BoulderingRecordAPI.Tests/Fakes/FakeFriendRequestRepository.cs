using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Repositories;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeFriendRequestRepository(IEnumerable<FriendRequest>? seedFriendRequests = null) : IFriendRequestRepository
{
    private readonly List<FriendRequest> _friendRequests = seedFriendRequests?.ToList() ?? [];

    public Task<FriendRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_friendRequests.FirstOrDefault(f => f.Id == id));

    public Task<FriendRequest?> GetBetweenUsersAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default)
        => Task.FromResult(_friendRequests.FirstOrDefault(f =>
            (f.RequesterId == userAId && f.AddresseeId == userBId) ||
            (f.RequesterId == userBId && f.AddresseeId == userAId)));

    public Task<List<FriendRequest>> GetPendingReceivedAsync(Guid addresseeId, CancellationToken cancellationToken = default)
        => Task.FromResult(_friendRequests
            .Where(f => f.AddresseeId == addresseeId && f.Status == FriendRequestStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToList());

    public Task<List<FriendRequest>> GetAcceptedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_friendRequests
            .Where(f => f.Status == FriendRequestStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .OrderByDescending(f => f.CreatedAt)
            .ToList());

    public Task<List<FriendRequest>> GetRelationsForUsersAsync(
        Guid currentUserId,
        IReadOnlyCollection<Guid> otherUserIds,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_friendRequests
            .Where(f =>
                (f.RequesterId == currentUserId && otherUserIds.Contains(f.AddresseeId)) ||
                (f.AddresseeId == currentUserId && otherUserIds.Contains(f.RequesterId)))
            .ToList());

    public Task AddAsync(FriendRequest friendRequest, CancellationToken cancellationToken = default)
    {
        _friendRequests.Add(friendRequest);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FriendRequest friendRequest, CancellationToken cancellationToken = default)
    {
        _friendRequests.RemoveAll(f => f.Id == friendRequest.Id);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
