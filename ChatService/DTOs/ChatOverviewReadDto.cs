namespace ChatService.DTOs;

public class ChatOverviewReadDto
{
    public string SenderId { get; set; }
    public string SenderUserName { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTimestamp { get; set; }
}