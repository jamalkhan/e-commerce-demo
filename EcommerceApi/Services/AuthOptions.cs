namespace EcommerceApi.Services;

public class AuthOptions
{
    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan PasswordResetTokenDuration { get; set; } = TimeSpan.FromHours(1);
    public string PasswordResetUrlTemplate { get; set; } = "https://sandbox.mvc.jamal.com/account/reset-password?token={token}";
    public string WelcomeFromName { get; set; } = "Ecommerce Demo";
}
