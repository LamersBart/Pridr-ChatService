using AutoMapper;
using ChatService.DTOs;
using ChatService.Models;

namespace ChatService.profiles;

public class ChatProfile : Profile
{
    public ChatProfile()
    {
        // source -> target
        CreateMap<ChatOverview, ChatOverviewReadDto>();
    }
}