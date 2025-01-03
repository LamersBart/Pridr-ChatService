using ChatService.Models;

namespace ChatService.Data;

public interface IMessageRepo
{
    void SaveMessage(Message message);
    void UpdateMessage(Message message);
    IEnumerable<Message> GetUndeliveredMessages(string userId);
    void MarkMessagesAsDelivered(IEnumerable<Message> messages);
    IEnumerable<Message> GetMessageHistory(string userId, string targetUserId);
    Task<bool> SaveChangesAsync();
    bool MessagesExists(string keycloakUserId);
    Task BulkUpdateMessagesFromUserAsync(string keycloakUserId, string text);
    // Task BulkUpdateUserNameFromUserAsync(string keycloakUserId, string userName);
}