using ChatService.Models;

namespace ChatService.Data;

public interface IChatOverviewRepo
{
    public IEnumerable<ChatOverview> GetChatList(string userId);
}