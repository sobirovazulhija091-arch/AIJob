using Domain.DTOs;
using Domain.Entities;

public interface IRecommendationService
{
    Task<Response<string>> CreateAsync(int authorId, CreateRecommendationDto dto);
    Task<Response<string>> DeleteAsync(int id, int userId);
    Task<Response<Recommendation>> GetByIdAsync(int id);
    Task<Response<List<Recommendation>>> GetByRecipientIdAsync(int recipientId);
}
