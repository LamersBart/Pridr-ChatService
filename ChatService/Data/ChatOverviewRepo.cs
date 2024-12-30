using ChatService.Data.Encryption;
using ChatService.Models;

namespace ChatService.Data;

public class ChatOverviewRepo : IChatOverviewRepo
{
    
    private readonly AppDbContext _context;

    public ChatOverviewRepo(AppDbContext context)
    {
        _context = context;
    }
    
    public IEnumerable<ChatOverview> GetChatList(string userId)
    {
        // Haal alle berichten op waarin de gebruiker betrokken is
        var messages = _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .ToList();

        // Groepeer berichten op gesprekspartner
        var chats = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId) // Groepeer op de gesprekspartner
            .Select(g =>
            {
                // Haal het laatste bericht binnen de groep op
                var lastMessage = g.OrderByDescending(m => m.Timestamp).First();

                // Bepaal gesprekspartner op basis van ingelogde gebruiker
                var partnerId = lastMessage.SenderId == userId ? lastMessage.ReceiverId : lastMessage.SenderId;
                var partnerName = lastMessage.SenderId == userId ? lastMessage.ReceiverUserName : lastMessage.SenderUserName;

                // Bouw ChatOverview object
                return new ChatOverview
                {
                    SenderId = partnerId, // ID van de gesprekspartner
                    SenderUserName = partnerName, // Naam van de gesprekspartner
                    LastMessage = lastMessage.MessageText, // Tekst van het laatste bericht
                    LastMessageTimestamp = lastMessage.Timestamp // Tijdstip van het laatste bericht
                };
            })
            .OrderByDescending(chat => chat.LastMessageTimestamp) // Sorteer op het laatst verstuurde bericht
            .ToList();

        return chats; // Geef de lijst terug
    }

}