public interface IConversationService
{
    Task<Response<Conversation>> GetOrCreateAsync(int userId, int otherUserId);
    Task<Response<List<Conversation>>> GetByUserIdAsync(int userId);
    Task<Response<Conversation>> GetByIdAsync(int id, int userId);
}
