using ChatService.Models;

namespace ChatService.Data;

public class UsernameRepo : IUsernameRepo
{
    private readonly AppDbContext _context;

    public UsernameRepo(AppDbContext context)
    {
        _context = context;
    }
    
    public bool SaveChanges()
    {
        return _context.SaveChanges() >= 0;
    }

    public void CreateUsername(Username username)
    {
        if (username is null)
        {
            throw new ArgumentNullException(nameof(username));
        }
        _context.Usernames.Add(username);
    }

    public bool UsernameExist(string keycloakId)
    {
        return _context.Usernames.Any(p => p.KeycloakId == keycloakId);
    }

    public Username GetUsernameById(string keycloakId)
    {
        return _context.Usernames.FirstOrDefault(p => p.KeycloakId == keycloakId)!;
    }

    public void UpdateUsername(Username username)
    {
        if (username is null)
        {
            throw new ArgumentNullException(nameof(username));
        }
        _context.Usernames.Update(username);
    }
}