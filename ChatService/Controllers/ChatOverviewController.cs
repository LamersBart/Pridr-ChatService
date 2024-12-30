using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using ChatService.Data;
using ChatService.DTOs;
using ChatService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/chatoverview")]
public class ChatOverviewController : ControllerBase
{
    private readonly IChatOverviewRepo _repo;
    private readonly IMapper _mapper;

    public ChatOverviewController(IChatOverviewRepo repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }
    
    [HttpGet()]
    public async Task<ActionResult<IEnumerable<ChatOverview>>> GetChatOverview() 
    {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        if (accessToken != null)
        {
            string keycloakUserId = FetchKeycloakUserId(accessToken);
            Console.WriteLine("--> Getting all Chats for user...");
            IEnumerable<ChatOverview> chats = _repo.GetChatList(keycloakUserId);
            if (chats == null || !chats.Any())
            {
                return NoContent();
            }
            return Ok(_mapper.Map<IEnumerable<ChatOverviewReadDto>>(chats));
        }
        return NotFound("Invalid token.");
    }
    
    private static Dictionary<string, object> DecodeJwt(string bearerToken)
    {
        try
        {
            // Strip "Bearer " als prefix
            var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

            // Lees het token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Claims verwerken
            var claims = new Dictionary<string, object>();

            foreach (var claim in jwtToken.Claims)
            {
                // Controleer of de sleutel al bestaat
                if (claims.ContainsKey(claim.Type))
                {
                    // Voeg meerdere waarden toe als lijst
                    if (claims[claim.Type] is List<object> list)
                    {
                        list.Add(claim.Value); // Voeg toe aan bestaande lijst
                    }
                    else
                    {
                        claims[claim.Type] = new List<object> { claims[claim.Type], claim.Value };
                    }
                }
                else
                {
                    // Voeg nieuwe sleutel/waarde toe
                    claims[claim.Type] = claim.Value;
                }
            }

            // Voeg standaardwaarden toe
            claims["iss"] = jwtToken.Issuer;
            claims["aud"] = jwtToken.Audiences.ToList(); // Audiences expliciet als lijst

            return claims;
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid token", ex);
        }
    }
    private static string FetchKeycloakUserId(string bearerToken)
    {
        Dictionary<string, object> dictionary = DecodeJwt(bearerToken);
        if (dictionary.TryGetValue("sub", out object? subValue))
        {
            return subValue.ToString() ?? throw new InvalidOperationException();
        }
        throw new InvalidOperationException();
    }
}