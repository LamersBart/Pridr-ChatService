using ChatService.Models;

namespace ChatService.Data;

public interface IMessageRepo
{
    void SaveMessage(Message message);
    IEnumerable<Message> GetUndeliveredMessages(string userId);
    void MarkMessagesAsDelivered(IEnumerable<Message> messages);
    IEnumerable<Message> GetMessageHistory(string userId, string targetUserId);
    Task<bool> SaveChangesAsync();
}