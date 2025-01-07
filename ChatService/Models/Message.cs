using System.ComponentModel.DataAnnotations;

namespace ChatService.Models;

public class Message
{
    [Key]
    public int Id { get; set; }
    public required string SenderId { get; set; } // Vereist bij aanmaken
    public required string ReceiverId { get; set; } // Vereist bij aanmaken
    public required string MessageText { get; set; } // Vereist bij aanmaken
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsDelivered { get; set; } = false;
}
