using Domain.DTOs;
using Infrastructure.Responses;

public interface IConversationService
{
    Task<Response<ConversationListItemDto>> GetOrCreateAsync(int userId, int otherUserId);
    Task<Response<List<ConversationListItemDto>>> GetByUserIdAsync(int userId);
    Task<Response<ConversationListItemDto>> GetByIdAsync(int id, int userId);
    Task<Response<string>> DeleteAsync(int conversationId, int userId);
}
