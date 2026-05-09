namespace EcommerceData.Entities;

public class Session
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActivityAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public User? User { get; set; }
}
