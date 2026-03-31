using Domain.DTOs;

public interface IMessageService
{
    Task<Response<string>> CreateAsync(int senderId, CreateMessageDto dto);
    Task<Response<Message>> GetByIdAsync(int id);
    Task<Response<List<Message>>> GetByConversationIdAsync(int conversationId, int userId);
    /// <summary>Any participant in the conversation may remove the message (removed for both users).</summary>
    Task<Response<string>> DeleteAsync(int messageId, int userId);
}