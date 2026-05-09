namespace EcommerceData.Entities;

public class PasswordResetToken
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public User? User { get; set; }
}
