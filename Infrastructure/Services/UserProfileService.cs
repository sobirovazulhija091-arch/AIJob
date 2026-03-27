using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class UserProfileService(ApplicationDbContext dbContext) : IUserProfileService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateUserProfileDto dto)
    {
        var profile = new UserProfile
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            AboutMe = dto.AboutMe,
            ExperienceYears = dto.ExperienceYears,
            ExpectedSalary = dto.ExpectedSalary,
            CVFileUrl = dto.CVFileUrl
        };
        await context.UserProfiles.AddAsync(profile);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add UserProfile successfully");
    }

    public async Task<Response<UserProfile>> GetByIdAsync(int id)
    {
        var get = await context.UserProfiles.FindAsync(id);
        if (get == null)
            return new Response<UserProfile>(HttpStatusCode.NotFound, "UserProfile not found");
        return new Response<UserProfile>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<UserProfile>>> GetAllAsync()
    {
        var list = await context.UserProfiles.ToListAsync();
        return new Response<List<UserProfile>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateUserProfileDto dto)
    {
        var update = await context.UserProfiles.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserProfile not found");

        update.UserId = dto.UserId;
        update.FirstName = dto.FirstName;
        update.LastName = dto.LastName;
        update.AboutMe = dto.AboutMe;
        update.ExperienceYears = dto.ExperienceYears;
        update.ExpectedSalary = dto.ExpectedSalary;
        update.CVFileUrl = dto.CVFileUrl;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.UserProfiles.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserProfile not found");

        context.UserProfiles.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted UserProfile successfully");
    }

    public async Task<Response<UserProfile>> GetByUserIdAsync(int userId)
    {
        var get = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (get == null)
            return new Response<UserProfile>(HttpStatusCode.NotFound, "UserProfile not found");
        return new Response<UserProfile>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<UserPublicProfileDto>>> GetPublicProfilesByUserIdsAsync(List<int> userIds)
    {
        var distinctIds = userIds.Distinct().ToList();
        if (distinctIds.Count == 0)
            return new Response<List<UserPublicProfileDto>>(HttpStatusCode.OK, "ok", []);

        var profiles = await context.UserProfiles
            .Where(p => distinctIds.Contains(p.UserId))
            .Select(p => new UserPublicProfileDto
            {
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FullName = (p.FirstName + " " + p.LastName).Trim()
            })
            .ToListAsync();

        return new Response<List<UserPublicProfileDto>>(HttpStatusCode.OK, "ok", profiles);
    }
}
