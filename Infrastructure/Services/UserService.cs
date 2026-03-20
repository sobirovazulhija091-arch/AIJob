using System.Net;
using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<User> userManager;

    public UserService(ApplicationDbContext dbContext, UserManager<User> userManager)
    {
        context = dbContext;
        this.userManager = userManager;
    }

    public async Task<Response<string>> CreateAsync(CreateUserDto dto)
    {
        var existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return new Response<string>(HttpStatusCode.BadRequest, "Email already in use");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return new Response<string>(HttpStatusCode.BadRequest, string.Join("; ", result.Errors.Select(e => e.Description)));

        return new Response<string>(HttpStatusCode.OK, "User created successfully");
    }

    public async Task<Response<User>> GetByIdAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return new Response<User>(HttpStatusCode.NotFound, "User not found");
        return new Response<User>(HttpStatusCode.OK, "ok", user);
    }

    public async Task<Response<List<User>>> GetAllAsync()
    {
        var list = await userManager.Users.ToListAsync();
        return new Response<List<User>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<PagedResult<User>> GetPagedAsync(UserFilter filter, PagedQuery querypage)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(u => (u.FullName != null && u.FullName.Contains(filter.Name)) || (u.UserName != null && u.UserName.Contains(filter.Name)));
        if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(u => u.Email != null && u.Email.Contains(filter.Email));

        var total = await query.CountAsync();
        var page = querypage.PageNumber > 0 ? querypage.PageNumber : 1;
        var pageSize = querypage.PageSize > 0 ? querypage.PageSize : 10;
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return new Response<string>(HttpStatusCode.NotFound, "User not found");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return new Response<string>(HttpStatusCode.NotFound, "User not found");

        await userManager.DeleteAsync(user);
        return new Response<string>(HttpStatusCode.OK, "Deleted User successfully");
    }

    public async Task<Response<User>> GetByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return new Response<User>(HttpStatusCode.NotFound, "User not found");
        return new Response<User>(HttpStatusCode.OK, "ok", user);
    }

    public async Task<Response<string>> ChangeRoleAsync(int id, UserRole role)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return new Response<string>(HttpStatusCode.NotFound, "User not found");

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, role.ToString());
        return new Response<string>(HttpStatusCode.OK, "Role changed successfully");
    }
}
