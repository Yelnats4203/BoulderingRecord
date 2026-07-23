namespace BoulderingRecordAPI.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Username { get; set; } = string.Empty;
    public string Acc { get; set; } = string.Empty;
    public string Psw { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
