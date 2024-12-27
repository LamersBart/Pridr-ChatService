using System.ComponentModel.DataAnnotations;

namespace ChatService.Models;

public class Connection
{
    [Key]
    public int Id { get; set; }
    public required string ConnectionId { get; set; }
    public required string UserId { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}
