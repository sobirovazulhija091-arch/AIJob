using System.Net;
using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class JobApplicationService(ApplicationDbContext dbContext) : IJobApplicationService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateJobApplicationDto dto)
    {
        var exists = await context.JobApplications.AnyAsync(ja => ja.JobId == dto.JobId && ja.UserId == dto.UserId);
        if (exists)
            return new Response<string>(HttpStatusCode.BadRequest, "Already applied to this job");

        var application = new JobApplication
        {
            JobId = dto.JobId,
            UserId = dto.UserId,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };
        await context.JobApplications.AddAsync(application);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Applied successfully");
    }

    public async Task<Response<JobApplication>> GetByIdAsync(int id)
    {
        var get = await context.JobApplications.FindAsync(id);
        if (get == null)
            return new Response<JobApplication>(HttpStatusCode.NotFound, "JobApplication not found");
        return new Response<JobApplication>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<JobApplication>>> GetAllAsync()
    {
        var list = await context.JobApplications.ToListAsync();
        return new Response<List<JobApplication>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<PagedResult<JobApplication>> GetPagedAsync(JobApplicationFilter filter, PagedQuery querypage)
    {
        var query = context.JobApplications.AsQueryable();
        if (filter.Status.HasValue)
            query = query.Where(ja => ja.Status == filter.Status.Value);
        if (filter.JobId.HasValue)
            query = query.Where(ja => ja.JobId == filter.JobId.Value);
        if (filter.UserId.HasValue)
            query = query.Where(ja => ja.UserId == filter.UserId.Value);

        var total = await query.CountAsync();
        var page = querypage.PageNumber > 0 ? querypage.PageNumber : 1;
        var pageSize = querypage.PageSize > 0 ? querypage.PageSize : 10;
        var jobList = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<JobApplication>
        {
            Items = jobList,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateJobApplicationDto dto)
    {
        var update = await context.JobApplications.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobApplication not found");

        update.JobId = dto.JobId;
        update.UserId = dto.UserId;
        update.Status = dto.Status;
        update.AppliedAt = dto.AppliedAt;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.JobApplications.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobApplication not found");

        context.JobApplications.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted JobApplication successfully");
    }

    public async Task<Response<List<JobApplication>>> GetByUserIdAsync(int userId)
    {
        var list = await context.JobApplications.Where(ja => ja.UserId == userId).ToListAsync();
        return new Response<List<JobApplication>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<List<JobApplication>>> GetByJobIdAsync(int jobId)
    {
        var list = await context.JobApplications.Where(ja => ja.JobId == jobId).ToListAsync();
        return new Response<List<JobApplication>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> ChangeStatusAsync(int id, ApplicationStatus status)
    {
        var app = await context.JobApplications.FindAsync(id);
        if (app == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobApplication not found");

        app.Status = status;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Status changed successfully");
    }
}
