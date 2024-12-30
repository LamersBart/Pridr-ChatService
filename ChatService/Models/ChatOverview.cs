using System.ComponentModel.DataAnnotations;

namespace ChatService.Models;

public class ChatOverview
{
    public string SenderId { get; set; }
    public string SenderUserName { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTimestamp { get; set; }
}
