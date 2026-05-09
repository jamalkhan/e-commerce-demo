namespace EcommerceMvc.Services;

public record AuthApiUser(int Id, string Email, string Name);

public record AuthApiSession(string Token, DateTimeOffset ExpiresAt, AuthApiUser User);

public record AuthApiError(string Message);

public class AuthApiResult<T>
{
    public AuthApiResult(T? value, string? errorMessage, int statusCode)
    {
        Value = value;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public T? Value { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }
    public bool IsSuccess => ErrorMessage is null && StatusCode >= 200 && StatusCode < 300;
}
