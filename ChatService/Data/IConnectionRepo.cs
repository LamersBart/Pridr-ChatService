using ChatService.Models;

namespace ChatService.Data;

public interface IConnectionRepo
{
    Task<bool> SaveChangesAsync();
    void CreateConnection(Connection connection);
    Connection GetConnectionById(string connectionId);
    IEnumerable<Connection> GetConnectionByUserId(string userId);
    Connection? GetTargetConnectionById(string targetUserId);
    void DeleteConnection(string connectionId);
    bool ConnectionExists(string connectionId);
}