using System.ComponentModel.DataAnnotations;

namespace ChatService.Models;

public class ChatOverview
{
    public required string PartnerId { get; set; }
    public required string PartnerUserName { get; set; }
    public required string LastMessage { get; set; }
    public DateTime LastMessageTimestamp { get; set; }
}
