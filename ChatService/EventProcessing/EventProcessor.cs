using System.Text.Json;
using ChatService.Data;
using ChatService.DTOs;
using ChatService.Enums;
using ChatService.Models;

namespace ChatService.EventProcessing;

public class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public void ProcessEvent(string message)
    {
        Console.WriteLine($"--> Event received");
        var eventType = DetermineEvent(message);
        switch (eventType)
        {
            case EventType.Register:
                Console.WriteLine($"--> Event: {message}");
                AddNewUsername(message);
                break;
            case EventType.UpdatedUsername:
                Console.WriteLine($"--> Event: {message}");
                UpdateUsername(message);
                break;
            case EventType.Delete:
                Console.WriteLine($"--> Event: {message}");
                UsernameDeleted(message);
                _ = DeleteUserContent(message);
                break;
            default: 
                break;
        }
    }

    private static EventType DetermineEvent(string notificationMessage)
    {
        Console.WriteLine("--> Determining Event");
        try
        {
            var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(notificationMessage);
            if (keycloakEvent != null)
            {
                switch (keycloakEvent.Type)
                {
                    case "LOGIN":
                        Console.WriteLine("--> Login Event Detected");
                        return EventType.Login;
                    case "LOGOUT":
                        Console.WriteLine("--> Logout Event Detected");
                        return EventType.Logout;
                    case "REGISTER":
                        Console.WriteLine("--> Register Event Detected");
                        return EventType.Register;
                    case "DELETE_ACCOUNT":
                        Console.WriteLine("--> Delete account Event Detected");
                        return EventType.Delete;
                    default:
                        Console.WriteLine("--> Other Event Detected");
                        return EventType.Undetermined;
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine("--> Not a Keycloak event, trying UserService event.");
        }
        
        try
        {
            var userServiceEvent = JsonSerializer.Deserialize<UserServiceEventDto>(notificationMessage);
            if (userServiceEvent != null && userServiceEvent.EventType == "Updated_UserName")
            {
                Console.WriteLine("--> Updated Username Event Detected");
                return EventType.UpdatedUsername;
            }
        }
        catch (JsonException)
        {
            Console.WriteLine("--> Could not determine event type.");
        }
        Console.WriteLine("--> Received Event is 'Undetermined'");
        return EventType.Undetermined;
    }

    private void AddNewUsername(string keyCloakPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var usernameRepo = scope.ServiceProvider.GetRequiredService<IUsernameRepo>();
            var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(keyCloakPublishedMessage);
            try
            {
                if (keycloakEvent != null)
                {
                    if (!usernameRepo.UsernameExist(keycloakEvent.UserId))
                    {
                        var username = new Username
                        {
                            KeycloakId = keycloakEvent.UserId,
                            UserName = ""
                        };
                        usernameRepo.CreateUsername(username);
                        usernameRepo.SaveChanges();
                    }
                }
                else
                {
                    Console.WriteLine($"--> Failed reading keycloak event {keyCloakPublishedMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not add new username to DB {ex.Message}");
            }
        }
    }
    private void UpdateUsername(string userServiceEventMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var usernameRepo = scope.ServiceProvider.GetRequiredService<IUsernameRepo>();
            var userServiceEvent = JsonSerializer.Deserialize<UserServiceEventDto>(userServiceEventMessage);
            try
            {
                if (userServiceEvent != null)
                {
                    if (!usernameRepo.UsernameExist(userServiceEvent.KeyCloakId))
                    {
                        Console.WriteLine($"--> No messages found for User {userServiceEvent.KeyCloakId}. Skipping deletion.");
                        return;
                    }
                    var username = usernameRepo.GetUsernameById(userServiceEvent.KeyCloakId);
                    username.UserName = userServiceEvent.UserName;
                    usernameRepo.UpdateUsername(username);
                    usernameRepo.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"--> Failed reading keycloak event {userServiceEventMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not remove messages created by the now deleted User from DB {ex.Message}");
            }
        }
    }
    private void UsernameDeleted(string keyCloakPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var usernameRepo = scope.ServiceProvider.GetRequiredService<IUsernameRepo>();
            var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(keyCloakPublishedMessage);
            try
            {
                if (keycloakEvent != null)
                {
                    if (!usernameRepo.UsernameExist(keycloakEvent.UserId))
                    {
                        Console.WriteLine($"--> No username found for {keycloakEvent.UserId}. Skipping deletion.");
                        return;
                    }
                    var username = usernameRepo.GetUsernameById(keycloakEvent.UserId);
                    username.UserName = "Deleted User";
                    usernameRepo.UpdateUsername(username);
                    usernameRepo.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"--> Failed reading keycloak event {keyCloakPublishedMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not update 'deleted' username to DB {ex.Message}");
            }
        }
    }
    private async Task DeleteUserContent(string keyCloakPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var messageRepo = scope.ServiceProvider.GetRequiredService<IMessageRepo>();
            var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(keyCloakPublishedMessage);
            try
            {
                if (keycloakEvent != null)
                {
                    if (!messageRepo.MessagesExists(keycloakEvent.UserId))
                    {
                        Console.WriteLine($"--> No messages found for User {keycloakEvent.UserId}. Skipping deletion.");
                        return;
                    }
                    await messageRepo.BulkUpdateMessagesFromUserAsync(keycloakEvent.UserId,"This message was deleted from the database.");
                    await messageRepo.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"--> Failed reading keycloak event {keyCloakPublishedMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not remove messages created by the now deleted User from DB {ex.Message}");
            }
        }
    }
}