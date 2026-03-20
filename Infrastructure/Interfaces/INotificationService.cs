using Domain.DTOs;
using Infrastructure.Responses;

public interface INotificationService
{
    Task<Response<string>> CreateAsync(CreateNotificationDto dto);
    Task<Response<Notification>> GetByIdAsync(int id);
    Task<Response<List<Notification>>> GetAllAsync();
    Task<PagedResult<Notification>> GetPagedAsync(int userId, PagedQuery querypage);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Notification>>> GetByUserIdAsync(int userId);
    Task<Response<string>> MarkAsReadAsync(int id);
}