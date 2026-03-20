using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class JobCategoryService(ApplicationDbContext dbContext) : IJobCategoryService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateJobCategoryDto dto)
    {
        var category = new JobCategory { Name = dto.Name };
        await context.JobCategories.AddAsync(category);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add JobCategory successfully");
    }

    public async Task<Response<JobCategory>> GetByIdAsync(int id)
    {
        var get = await context.JobCategories.FindAsync(id);
        if (get == null)
            return new Response<JobCategory>(HttpStatusCode.NotFound, "JobCategory not found");
        return new Response<JobCategory>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<JobCategory>>> GetAllAsync()
    {
        var list = await context.JobCategories.ToListAsync();
        return new Response<List<JobCategory>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateJobCategoryDto dto)
    {
        var update = await context.JobCategories.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobCategory not found");

        update.Name = dto.Name;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.JobCategories.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobCategory not found");

        context.JobCategories.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted JobCategory successfully");
    }
}
