namespace BoulderingRecordAPI.Repositories;

public interface IGymRepository
{
    Task<List<string>> GetDistinctGymNamesAsync(CancellationToken cancellationToken = default);
}
