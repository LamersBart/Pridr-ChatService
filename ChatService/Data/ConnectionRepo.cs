using ChatService.Models;

namespace ChatService.Data;

public class ConnectionRepo : IConnectionRepo
{
    private readonly AppDbContext _context;
    
    public ConnectionRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() >= 0;
    }

    public void CreateConnection(Connection connection)
    {
        if (connection is null)
        {
            throw new ArgumentNullException(nameof(connection));
        }
        _context.Connections.Add(connection);
    }

    public Connection GetConnectionById(string connectionId)
    {
        return _context.Connections.FirstOrDefault(p => p.ConnectionId == connectionId)!;
    }

    public Connection? GetTargetConnectionById(string targetUserId)
    {
        var result = _context.Connections.FirstOrDefault(c => c.UserId == targetUserId);
        Console.WriteLine(result == null ? "No connection found." : $"Connection found: {result.ConnectionId}");
        return result;
    }

    public void DeleteConnection(string connectionId)
    {
        Connection connection = GetConnectionById(connectionId);
        if (connection != null)
        {
            _context.Connections.Remove(connection);
        }
    }

    public bool ConnectionExists(string connectionId)
    {
        return _context.Connections.Any(p => p.ConnectionId == connectionId);
    }
}