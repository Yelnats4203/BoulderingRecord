namespace BoulderingRecordAPI.Repositories;

public interface IGymRepository
{
    /// <summary>
    /// 刻意回傳全站（不限使用者）去重後的岩館名稱清單，供 autocomplete 建議使用；
    /// 不含使用者身分或其他攀岩紀錄欄位，實作不應加入依使用者過濾的邏輯。
    /// </summary>
    Task<List<string>> GetDistinctGymNamesAsync(CancellationToken cancellationToken = default);
}
