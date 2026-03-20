using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IUserService
{
    Task<Response<string>> CreateAsync(CreateUserDto dto);
    Task<Response<User>> GetByIdAsync(int id);
    Task<Response<List<User>>> GetAllAsync();
    Task<PagedResult<User>> GetPagedAsync(UserFilter filter, PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateUserDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<User>> GetByEmailAsync(string email);
    Task<Response<string>> ChangeRoleAsync(int id, UserRole role);
}