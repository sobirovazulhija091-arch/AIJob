using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IUserService
{
    Task<Response<string>> CreateAsync(CreateUserDto dto);
    Task<Response<UserResponseDto>> GetByIdAsync(int id);
    Task<Response<List<UserResponseDto>>> GetAllAsync();
    Task<PagedResult<UserResponseDto>> GetPagedAsync(UserFilter filter, PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateUserDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<UserResponseDto>> GetByEmailAsync(string email);
    Task<Response<string>> ChangeRoleAsync(int id, UserRole role);
    Task<Response<List<MemberDirectoryEntryDto>>> GetMemberDirectoryAsync(int? excludeUserId);
}