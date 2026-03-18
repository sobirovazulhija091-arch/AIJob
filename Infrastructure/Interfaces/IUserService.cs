using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IUserService
{
    Task<Response<UserResponseDto>> CreateAsync(CreateUserDto dto);
    Task<Response<UserResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<UserResponseDto>>> GetAllAsync();
    Task<Response<PagedResult<UserResponseDto>>> GetPagedAsync(UserFilter filter);
    Task<Response<UserResponseDto>> UpdateAsync(int id, UpdateUserDto dto);
    Task<Response<UserResponseDto>> DeleteAsync(int id);

    Task<Response<UserResponseDto>> GetByEmailAsync(string email);
    Task<Response<bool>> ChangeRoleAsync(int id, UserRole role);
}