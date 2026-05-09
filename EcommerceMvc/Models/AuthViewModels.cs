using System.ComponentModel.DataAnnotations;

namespace EcommerceMvc.Models;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required, MinLength(8)]
    public string? Password { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string? Email { get; set; }
}

public class ResetPasswordViewModel
{
    [Required]
    public string? Token { get; set; }

    [Required, MinLength(8)]
    public string? NewPassword { get; set; }
}
