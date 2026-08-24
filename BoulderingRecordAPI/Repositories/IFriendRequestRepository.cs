using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Repositories;

public interface IFriendRequestRepository
{
    Task<FriendRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<FriendRequest?> GetBetweenUsersAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default);

    Task<List<FriendRequest>> GetPendingReceivedAsync(Guid addresseeId, CancellationToken cancellationToken = default);

    Task<List<FriendRequest>> GetAcceptedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<List<FriendRequest>> GetRelationsForUsersAsync(
        Guid currentUserId,
        IReadOnlyCollection<Guid> otherUserIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(FriendRequest friendRequest, CancellationToken cancellationToken = default);

    Task DeleteAsync(FriendRequest friendRequest, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
