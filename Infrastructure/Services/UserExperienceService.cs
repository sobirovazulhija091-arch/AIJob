using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class UserExperienceService(ApplicationDbContext dbContext) : IUserExperienceService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateUserExperienceDto dto)
    {
        var userExperience = new UserExperience
        {
            UserId = dto.UserId,
            CompanyName = dto.CompanyName,
            Position = dto.Position,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };
        await context.UserExperiences.AddAsync(userExperience);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add UserExperience successfully");
    }

    public async Task<Response<UserExperience>> GetByIdAsync(int id)
    {
        var get = await context.UserExperiences.FindAsync(id);
        if (get == null)
            return new Response<UserExperience>(HttpStatusCode.NotFound, "UserExperience not found");
        return new Response<UserExperience>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<UserExperience>>> GetAllAsync()
    {
        var list = await context.UserExperiences.ToListAsync();
        return new Response<List<UserExperience>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateUserExperienceDto dto)
    {
        var update = await context.UserExperiences.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserExperience not found");

        update.CompanyName = dto.CompanyName;
        update.Position = dto.Position;
        update.StartDate = dto.StartDate;
        update.EndDate = dto.EndDate;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.UserExperiences.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserExperience not found");

        context.UserExperiences.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted UserExperience successfully");
    }

    public async Task<Response<List<UserExperience>>> GetByUserIdAsync(int userId)
    {
        var list = await context.UserExperiences.Where(e => e.UserId == userId).ToListAsync();
        return new Response<List<UserExperience>>(HttpStatusCode.OK, "ok", list);
    }
}
