using Domain.DTOs;
using Infrastructure.Responses;

public interface IUserSettingsService
{
    Task<Response<UserSettingsDto>> GetByUserIdAsync(int userId);
    Task<Response<string>> UpdateByUserIdAsync(int userId, UpdateUserSettingsDto dto);
}
