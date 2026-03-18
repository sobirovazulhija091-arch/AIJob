using Domain.DTOs;
using Infrastructure.Responses;

public interface INotificationService
{
    Task<Response<NotificationDto>> CreateAsync(CreateNotificationDto dto);
    Task<Response<NotificationDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<NotificationDto>>> GetAllAsync();
    Task<Response<PagedResult<NotificationDto>>> GetPagedAsync(int userId, PagedRequest request);
    Task<Response<bool>> DeleteAsync(int id);

    Task<Response<IEnumerable<NotificationDto>>> GetByUserIdAsync(int userId);
    Task<Response<bool>> MarkAsReadAsync(int id);
}