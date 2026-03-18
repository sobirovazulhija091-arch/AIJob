using Domain.DTOs;

public interface IMessageService
{
    Task<Response<MessageDto>> CreateAsync(CreateMessageDto dto);
    Task<Response<MessageDto>> GetByIdAsync(int id);
    Task<Response<List<MessageDto>>> GetAllAsync();
    Task<Response<bool>> DeleteAsync(int id);

    Task<Response<List<MessageDto>>> GetByConversationIdAsync(int conversationId);
}