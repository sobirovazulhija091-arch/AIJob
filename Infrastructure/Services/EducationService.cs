using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class EducationService(ApplicationDbContext dbContext) : IEducationService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> AddAsync(CreateEducationDto dto)
    {
        var education = new Education
        {
            ProfileId = dto.ProfileId,
            SchoolName = dto.SchoolName,
            Degree = dto.Degree,
            FieldOfStudy = dto.FieldOfStudy,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Grade = dto.Grade,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.Educations.AddAsync(education);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add Education successfully");
    }

    public async Task<Response<Education>> GetByIdAsync(int id)
    {
        var get = await context.Educations.FindAsync(id);
        if (get == null)
            return new Response<Education>(HttpStatusCode.NotFound, "Education not found");
        return new Response<Education>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Education>>> GetAllAsync()
    {
        var list = await context.Educations.ToListAsync();
        return new Response<List<Education>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateEducationDto dto)
    {
        var update = await context.Educations.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "Education not found");

        update.SchoolName = dto.SchoolName;
        update.Degree = dto.Degree;
        update.FieldOfStudy = dto.FieldOfStudy;
        update.StartDate = dto.StartDate;
        update.EndDate = dto.EndDate;
        update.Grade = dto.Grade;
        update.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.Educations.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "Education not found");

        context.Educations.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted Education successfully");
    }

    public async Task<Response<List<Education>>> GetByProfileIdAsync(int profileId)
    {
        var list = await context.Educations.Where(x => x.ProfileId == profileId).ToListAsync();
        return new Response<List<Education>>(HttpStatusCode.OK, "ok", list);
    }

    
}
