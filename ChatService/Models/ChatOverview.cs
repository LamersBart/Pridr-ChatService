using System.ComponentModel.DataAnnotations;

namespace ChatService.Models;

public class ChatOverview
{
    public string PartnerId { get; set; }
    public string PartnerUserName { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTimestamp { get; set; }
}
