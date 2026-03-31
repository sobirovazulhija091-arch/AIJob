using System.Net;
using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class UserMappings
{
    public static UserResponseDto ToResponseDto(this User user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<User> userManager;

    public UserService(ApplicationDbContext dbContext, UserManager<User> userManager)
    {
        context = dbContext;
        this.userManager = userManager;
    }

    private static string PickPrimaryRole(IList<string> roles)
    {
        if (roles.Contains("Organization")) return "Organization";
        if (roles.Contains("Candidate")) return "Candidate";
        return roles.Count > 0 ? roles[0]! : "Candidate";
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

    public async Task<Response<UserResponseDto>> GetByIdAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return new Response<UserResponseDto>(HttpStatusCode.NotFound, "User not found");
        var dto = user.ToResponseDto();
        dto.AccountRole = PickPrimaryRole(await userManager.GetRolesAsync(user));
        return new Response<UserResponseDto>(HttpStatusCode.OK, "ok", dto);
    }

    public async Task<Response<List<UserResponseDto>>> GetAllAsync()
    {
        var users = await userManager.Users.ToListAsync();
        var list = new List<UserResponseDto>(users.Count);
        foreach (var u in users)
        {
            var dto = u.ToResponseDto();
            dto.AccountRole = PickPrimaryRole(await userManager.GetRolesAsync(u));
            list.Add(dto);
        }

        return new Response<List<UserResponseDto>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<PagedResult<UserResponseDto>> GetPagedAsync(UserFilter filter, PagedQuery querypage)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(u => (u.FullName != null && u.FullName.Contains(filter.Name)) || (u.UserName != null && u.UserName.Contains(filter.Name)));
        if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(u => u.Email != null && u.Email.Contains(filter.Email));

        var total = await query.CountAsync();
        var page = querypage.PageNumber > 0 ? querypage.PageNumber : 1;
        var pageSize = querypage.PageSize > 0 ? querypage.PageSize : 10;
        var pageUsers = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = new List<UserResponseDto>(pageUsers.Count);
        foreach (var u in pageUsers)
        {
            var dto = u.ToResponseDto();
            dto.AccountRole = PickPrimaryRole(await userManager.GetRolesAsync(u));
            items.Add(dto);
        }

        return new PagedResult<UserResponseDto>
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

    public async Task<Response<UserResponseDto>> GetByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return new Response<UserResponseDto>(HttpStatusCode.NotFound, "User not found");
        var dto = user.ToResponseDto();
        dto.AccountRole = PickPrimaryRole(await userManager.GetRolesAsync(user));
        return new Response<UserResponseDto>(HttpStatusCode.OK, "ok", dto);
    }

    public async Task<Response<List<MemberDirectoryEntryDto>>> GetMemberDirectoryAsync(int? excludeUserId)
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();
        var list = new List<MemberDirectoryEntryDto>();
        foreach (var u in users)
        {
            if (excludeUserId is { } x && u.Id == x) continue;
            var roles = await userManager.GetRolesAsync(u);
            var fn = string.IsNullOrWhiteSpace(u.FullName) ? null : u.FullName;
            list.Add(
                new MemberDirectoryEntryDto
                {
                    Id = u.Id,
                    FullName = fn,
                    UserName = u.UserName,
                    Email = u.Email,
                    Role = PickPrimaryRole(roles),
                });
        }

        static string SortKey(MemberDirectoryEntryDto e) =>
            e.FullName ?? e.UserName ?? e.Email ?? "";

        list.Sort((a, b) => string.Compare(SortKey(a), SortKey(b), StringComparison.OrdinalIgnoreCase));
        return new Response<List<MemberDirectoryEntryDto>>(HttpStatusCode.OK, "ok", list);
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
