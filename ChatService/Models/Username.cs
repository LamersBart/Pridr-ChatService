using System.ComponentModel.DataAnnotations;

namespace ChatService.Models;

public class Username
{
    [Key]
    public required string KeycloakId { get; set; }
    public required string UserName { get; set; }
}