namespace ChatService.DTOs;

public class ChatOverviewReadDto
{
    public string PartnerId { get; set; }
    public string PartnerUserName { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTimestamp { get; set; }
}