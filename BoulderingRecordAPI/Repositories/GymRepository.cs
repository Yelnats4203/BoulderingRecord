using BoulderingRecordAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Repositories;

public class GymRepository(BoulderingRecordDbContext dbContext) : IGymRepository
{
    /// <summary>
    /// 刻意查詢全站（不限使用者）的岩館名稱，供 autocomplete 建議使用，
    /// 讓使用者輸入時能沿用社群已使用過的名稱拼寫，避免同一岩館出現多種寫法。
    /// 回傳內容僅為去重後的名稱字串，不含使用者身分或其他攀岩紀錄欄位，
    /// 不應在此加入依 UserId 過濾的邏輯。
    /// </summary>
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
