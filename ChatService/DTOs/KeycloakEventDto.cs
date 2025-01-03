using System.Text.Json.Serialization;

namespace ChatService.DTOs;

public class KeycloakEventDto
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("realmId")]
    public required string RealmId { get; set; }
    [JsonPropertyName("clientId")]
    public required string ClientId { get; set; }
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }
    [JsonPropertyName("ipAddress")]
    public required string IpAddress { get; set; }
    [JsonPropertyName("details")]
    public required Details Details { get; set; }
}

public class Details
{
    [JsonPropertyName("auth_method")]
    public string? AuthMethod { get; set; }
    [JsonPropertyName("auth_type")]
    public string? AuthType { get; set; }
    [JsonPropertyName("register_method")]
    public string? RegisterMethod { get; set; }
    [JsonPropertyName("redirect_uri")]
    public string? RedirectUri { get; set; }
    [JsonPropertyName("code_id")]
    public string? CodeId { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
