namespace EcommerceApi.Models;

public record RegisterRequest(string? Email, string? Name, string? Password);

public record LoginCredentialsRequest(string? Email, string? Password);

public record ForgotPasswordRequest(string? Email);

public record ResetPasswordRequest(string? Token, string? NewPassword);

public record SessionResponse(string Token, DateTimeOffset ExpiresAt, UserDto User);

public record UserDto(int Id, string Email, string Name);
