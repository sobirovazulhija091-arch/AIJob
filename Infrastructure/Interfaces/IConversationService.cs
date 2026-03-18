using Domain.DTOs;

public interface IConversationService
{
    Task<Response<ConversationDto>> CreateAsync(CreateConversationDto dto);
    Task<Response<ConversationDto>> GetByIdAsync(int id);
    Task<Response<List<ConversationDto>>> GetAllAsync();
    Task<Response<bool>> DeleteAsync(int id);

    Task<Response<List<ConversationDto>>> GetByUserIdAsync(int userId);
}