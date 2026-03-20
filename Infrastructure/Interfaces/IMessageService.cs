using Domain.DTOs;

public interface IMessageService
{
    Task<Response<string>> CreateAsync(int senderId, CreateMessageDto dto);
    Task<Response<Message>> GetByIdAsync(int id);
    Task<Response<List<Message>>> GetByConversationIdAsync(int conversationId, int userId);
}