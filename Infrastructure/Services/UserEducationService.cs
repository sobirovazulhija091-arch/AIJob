using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class UserEducationService(ApplicationDbContext dbContext) : IUserEducationService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateUserEducationDto dto)
    {
        var userEducation = new UserEducation
        {
            UserId = dto.UserId,
            Institution = dto.Institution,
            Degree = dto.Degree,
            StartYear = dto.StartYear,
            EndYear = dto.EndYear
        };
        await context.UserEducations.AddAsync(userEducation);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add UserEducation successfully");
    }

    public async Task<Response<UserEducation>> GetByIdAsync(int id)
    {
        var get = await context.UserEducations.FindAsync(id);
        if (get == null)
            return new Response<UserEducation>(HttpStatusCode.NotFound, "UserEducation not found");
        return new Response<UserEducation>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<UserEducation>>> GetAllAsync()
    {
        var list = await context.UserEducations.ToListAsync();
        return new Response<List<UserEducation>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateUserEducationDto dto)
    {
        var update = await context.UserEducations.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserEducation not found");

        update.Institution = dto.Institution;
        update.Degree = dto.Degree;
        update.StartYear = dto.StartYear;
        update.EndYear = dto.EndYear;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.UserEducations.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserEducation not found");

        context.UserEducations.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted UserEducation successfully");
    }

    public async Task<Response<List<UserEducation>>> GetByUserIdAsync(int userId)
    {
        var list = await context.UserEducations.Where(e => e.UserId == userId).ToListAsync();
        return new Response<List<UserEducation>>(HttpStatusCode.OK, "ok", list);
    }
}
