using BoulderingRecordAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class GymRepository(BoulderingRecordDbContext dbContext) : IGymRepository
{
    public async Task<List<string>> GetDistinctGymNamesAsync(CancellationToken cancellationToken = default)
    {
        List<string> sendGymNames = await dbContext.Sends
            .Where(s => s.GymName != null && s.GymName != "")
            .Select(s => s.GymName!)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<string> sessionGymNames = await dbContext.Sessions
            .Where(s => s.GymName != null && s.GymName != "")
            .Select(s => s.GymName!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return sendGymNames
            .Concat(sessionGymNames)
            .Distinct()
            .OrderBy(name => name, StringComparer.CurrentCulture)
            .ToList();
    }
}
