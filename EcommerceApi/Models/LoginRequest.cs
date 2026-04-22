using System.Text.Json.Serialization;

namespace EcommerceApi.Models;

public record LoginRequest(
    [property: JsonPropertyName("userName")] string? UserName
);
