namespace EcommerceData.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<UserExternalLogin> ExternalLogins { get; set; } = new();
}
