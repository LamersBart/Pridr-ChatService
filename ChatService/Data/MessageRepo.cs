using ChatService.Data.Encryption;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Data;

public class MessageRepo : IMessageRepo
{
    private readonly AppDbContext _context;

    public MessageRepo(AppDbContext context)
    {
        _context = context;
    }

    public void SaveMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void UpdateMessage(Message message)
    {
        _context.Messages.Update(message);
    }

    public IEnumerable<Message> GetUndeliveredMessages(string userId)
    {
        return _context.Messages.Where(m => m.ReceiverId == userId && !m.IsDelivered).ToList();
    }

    public void MarkMessagesAsDelivered(IEnumerable<Message> messages)
    {
        foreach (var message in messages)
        {
            message.IsDelivered = true;
        }
    }

    public IEnumerable<Message> GetMessageHistory(string userId, string targetUserId)
    {
        // Haal berichten op tussen twee gebruikers, gesorteerd op tijdstip
        return _context.Messages
            .Where(m => 
                (m.SenderId == userId && m.ReceiverId == targetUserId) || 
                (m.SenderId == targetUserId && m.ReceiverId == userId))
            .OrderBy(m => m.Timestamp)
            .ToList();
    }
    
    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() >= 0;
    }

    public bool MessagesExists(string keycloakUserId)
    {
        return _context.Messages.Any(m => m.SenderId == keycloakUserId || m.ReceiverId == keycloakUserId);
    }
    
    public async Task BulkUpdateMessagesFromUserAsync(string keycloakUserId, string text)
    {
        await _context.Messages
            .Where(m => m.SenderId == keycloakUserId)
            .ExecuteUpdateAsync(m => m
                .SetProperty(p => p.MessageText, text));
    }
}