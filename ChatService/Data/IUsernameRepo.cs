using ChatService.Models;

namespace ChatService.Data;

public interface IUsernameRepo
{
    bool SaveChanges();
    void CreateUsername(Username username);
    bool UsernameExist(string keycloakId);
    Username GetUsernameById(string keycloakId);
    void UpdateUsername(Username username);
}