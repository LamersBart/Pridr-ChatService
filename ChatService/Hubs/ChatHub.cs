// using ChatService.Data;
// using ChatService.Models;
// using Microsoft.AspNetCore.SignalR;
//
// namespace ChatService.Hubs;
//
// public class ChatHub : Hub
// {
//     private readonly AppDbContext _dbContext;
//
//     public ChatHub(AppDbContext dbContext)
//     {
//         _dbContext = dbContext;
//     }
//     public async Task JoinChat(UserConnection connection)
//     {
//         _dbContext.Connections[Context.ConnectionId] = connection;
//         await Clients.All
//             .SendAsync("ReceiveMessage", "admin", $"{connection.UserId} joined the chat");
//     }
//
//     public async Task JoinSpecificChatRoom(UserConnection connection)
//     {
//         
//         await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
//         _dbContext.Connections[Context.ConnectionId] = connection; // save connection in DB
//         
//         // Notify group that the user joined
//         await Clients.Group(connection.ChatRoom)
//             .SendAsync("JoinSpecificChatRoom", "admin", $"{connection.UserId} has joined {connection.ChatRoom}");
//     }
//
//     public async Task ReceiveSpecificMessage(string msg)
//     {
//         if (_dbContext.Connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
//         {
//             await Clients.Group(connection.ChatRoom)
//                 .SendAsync("ReceiveSpecificMessage", connection.UserId, msg);
//         }
//     }
//     
//     public override async Task OnDisconnectedAsync(Exception? exception)
//     {
//         if (_dbContext.Connections.TryRemove(Context.ConnectionId, out var connection))
//         {
//             // Notify group that the user left
//             await Clients.Group(connection.ChatRoom)
//                 .SendAsync("ReceiveSpecificMessage", "admin", $"{connection.UserId} has left {connection.ChatRoom}");
//
//             await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);
//         }
//
//         await base.OnDisconnectedAsync(exception);
//     }
// }

using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ChatService.Data;
using ChatService.Models;

namespace ChatService.Hubs;
public class ChatHub : Hub
{
    private readonly IConnectionRepo _connectionRepo;
    private readonly IMessageRepo _messageRepo;

    public ChatHub(IConnectionRepo connectionRepo, IMessageRepo messageRepo)
    {
        _connectionRepo = connectionRepo;
        _messageRepo = messageRepo;
    }


    // Registreer gebruiker bij verbinding
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is null, aborting connection.");
                Context.Abort();
                return;
            }

            Console.WriteLine($"User ID: {userId}");

            if (!_connectionRepo.ConnectionExists(Context.ConnectionId))
            {
                var connection = new Connection
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    ConnectedAt = DateTime.UtcNow
                };
                _connectionRepo.CreateConnection(connection);

                await _connectionRepo.SaveChangesAsync();
                Console.WriteLine($"Connection saved for {userId}.");
            }

            // Lever offline berichten
            var undeliveredMessages = _messageRepo.GetUndeliveredMessages(userId);
            Console.WriteLine($"Offline messages: {undeliveredMessages.Count()}");

            foreach (var msg in undeliveredMessages)
            {
                await Clients.Caller.SendAsync("ReceiveDirectMessage", msg.SenderId, msg.MessageText, msg.Timestamp);
            }

            _messageRepo.MarkMessagesAsDelivered(undeliveredMessages);
            await _messageRepo.SaveChangesAsync();
            Console.WriteLine($"Delivered offline messages to {userId}.");

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            throw;
        }
    }
    
    // Haal chat-geschiedenis op uit de database 
    public async Task LoadFullMessageHistory(string targetUserId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "Authentication required.");
                return;
            }

            Console.WriteLine($"Loading full history between {userId} and {targetUserId}");

            // Haal alle berichten op tussen de twee gebruikers
            var messages = _messageRepo.GetMessageHistory(userId, targetUserId);

            // Verstuur de berichten naar de client
            foreach (var msg in messages)
            {
                await Clients.Caller.SendAsync("ReceiveDirectMessage", msg.SenderId, msg.MessageText, msg.Timestamp);
            }

            Console.WriteLine($"Full message history loaded between {userId} and {targetUserId}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadFullMessageHistory: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Error loading full message history.");
        }
    }


    // Verstuur direct bericht naar een andere gebruiker
    public async Task SendDirectMessage(string targetUserId, string message)
    {
        try
        {
            Console.WriteLine($"SendDirectMessage: Sender={Context.ConnectionId}, Target={targetUserId}, Message={message}");

            // Haal de afzender-ID op
            var senderUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(senderUserId))
            {
                Console.WriteLine("Sender ID is null.");
                await Clients.Caller.SendAsync("Error", "Authentication required.");
                return;
            }

            // Controleer of de ontvanger online is
            var targetConnection = _connectionRepo.GetTargetConnectionById(targetUserId);
            Console.WriteLine($"Target Connection: {targetConnection?.ConnectionId}");

            DateTime now = DateTime.UtcNow;
            // Maak het bericht aan (ongeacht online status)
            var newMessage = new Message
            {
                SenderId = senderUserId,
                ReceiverId = targetUserId,
                MessageText = message,
                Timestamp = now,
                IsDelivered = targetConnection != null // True als ontvanger online is, anders false
            };

            // Sla het bericht op in de database (altijd!)
            _messageRepo.SaveMessage(newMessage);
            await _messageRepo.SaveChangesAsync();
            Console.WriteLine("Message saved to database.");

            // Verstuur bericht direct als ontvanger online is
            if (targetConnection != null)
            {
                await Clients.Client(targetConnection.ConnectionId)
                    .SendAsync("ReceiveDirectMessage", senderUserId, message, now);
                Console.WriteLine("Message delivered to online user.");
            }

            // Bevestiging naar verzender
            await Clients.Caller.SendAsync("MessageDelivered", targetUserId, message);
            Console.WriteLine("Message delivery confirmed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendDirectMessage: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
            throw; // Herhaal fout voor debugging
        }
    }


    
    // Disconnect en cleanup
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"OnDisconnectedAsync called for {Context.ConnectionId}. Reason: {exception?.Message}");
        try
        {
            var connection = _connectionRepo.GetConnectionById(Context.ConnectionId);
            if (connection != null)
            {
                _connectionRepo.DeleteConnection(Context.ConnectionId);
                await _connectionRepo.SaveChangesAsync();
                Console.WriteLine($"Connection {Context.ConnectionId} removed.");
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            throw;
        }
    }

}
