using System.Text.Json.Serialization;

namespace ChatService.DTOs;

public class UserServiceEventDto
{
    [JsonPropertyName("KeyCloakId")]
    public required string KeyCloakId { get; set; }
    [JsonPropertyName("UserName")]
    public required string UserName { get; set; }
    [JsonPropertyName("EventType")]
    public required string EventType { get; set; }
}
