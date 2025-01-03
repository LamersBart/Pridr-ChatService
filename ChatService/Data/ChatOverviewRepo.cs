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

        // Haal alle unieke gesprekspartners op
        var partnerIds = messages
            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Distinct()
            .ToList();

        // Haal alle gebruikersnamen op van de gesprekspartners
        var usernames = _context.Usernames
            .Where(u => partnerIds.Contains(u.KeycloakId))
            .ToDictionary(u => u.KeycloakId, u => u.UserName);

        // Groepeer berichten op gesprekspartner
        var chats = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId) // Groepeer op de gesprekspartner
            .Select(g =>
            {
                // Haal het laatste bericht binnen de groep op
                var lastMessage = g.OrderByDescending(m => m.Timestamp).First();

                // Bepaal gesprekspartner op basis van ingelogde gebruiker
                var partnerId = lastMessage.SenderId == userId ? lastMessage.ReceiverId : lastMessage.SenderId;

                // Zoek de gebruikersnaam van de partner op
                var partnerName = usernames.ContainsKey(partnerId) ? usernames[partnerId] : "Onbekend";

                // Bouw ChatOverview object
                return new ChatOverview
                {
                    PartnerId = partnerId, // ID van de gesprekspartner
                    PartnerUserName = partnerName, // Naam van de gesprekspartner
                    LastMessage = lastMessage.MessageText, // Tekst van het laatste bericht
                    LastMessageTimestamp = lastMessage.Timestamp // Tijdstip van het laatste bericht
                };
            })
            .OrderByDescending(chat => chat.LastMessageTimestamp) // Sorteer op het laatst verstuurde bericht
            .ToList();

        return chats; // Geef de lijst terug
    }
}