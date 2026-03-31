using System.Net;
using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class JobService(ApplicationDbContext dbContext):IJobService
{
    private readonly ApplicationDbContext context = dbContext;

    private Task<bool> IsOrgMemberAsync(int userId, int organizationId) =>
        context.OrganizationMembers.AnyAsync(m => m.UserId == userId && m.OrganizationId == organizationId);

    public async Task<Response<string>> AddAsync(CreateJobDto dto, int actingUserId)
    {
        if (!await IsOrgMemberAsync(actingUserId, dto.OrganizationId))
            return new Response<string>(HttpStatusCode.Forbidden, "You are not a member of this organization");

       var  job = new Job
       {
           OrganizationId = dto.OrganizationId,
           Description = dto.Description,
           Title = dto.Title,
           SalaryMin = dto.SalaryMin,
           SalaryMax = dto.SalaryMax,
           Location  = dto.Location,
           JobType = dto.JobType,   
           ExperienceLevel = dto.ExperienceLevel,
           ExperienceRequired = dto.ExperienceRequired,
           CategoryId = dto.CategoryId
       };
       await context.Jobs.AddAsync(job);
       await context.SaveChangesAsync();
       return new Response<string>(HttpStatusCode.OK,"Add Job  successfully");
    }

    public async Task<Response<string>> DeleteAsync(int id, int actingUserId)
    {
        var del = await context.Jobs.FindAsync(id);
        if (del == null)
        {
            return new Response<string>(HttpStatusCode.NotFound,"Job not found");
        }
        if (!await IsOrgMemberAsync(actingUserId, del.OrganizationId))
            return new Response<string>(HttpStatusCode.Forbidden, "You cannot delete this job");
        context.Jobs.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK,"Deleted Job successfully");
    }

    public async Task<Response<List<Job>>> GetAllAsync()
    {
       return new Response<List<Job>>(HttpStatusCode.OK,"ok", await context.Jobs.ToListAsync());
    }

    public async Task<Response<List<Job>>> GetForUserAsync(int actingUserId)
    {
        var orgIds = await context.OrganizationMembers
            .Where(m => m.UserId == actingUserId)
            .Select(m => m.OrganizationId)
            .Distinct()
            .ToListAsync();
        if (orgIds.Count == 0)
            return new Response<List<Job>>(HttpStatusCode.OK, "ok", []);
        var jobs = await context.Jobs.Where(j => orgIds.Contains(j.OrganizationId)).ToListAsync();
        return new Response<List<Job>>(HttpStatusCode.OK, "ok", jobs);
    }

    public async Task<Response<Job>> GetByIdAsync(int id)
    {
        var get = await context.Jobs.FindAsync(id);
        if (get == null)
        {
            return new Response<Job>(HttpStatusCode.NotFound,"Job not found");
        }
        return new Response<Job>(HttpStatusCode.OK,"ok",get);
    }

    public async Task<Response<List<Job>>> GetByOrganizationIdAsync(int organizationId)
    {
        var jobs = await context.Jobs.Where(j => j.OrganizationId == organizationId).ToListAsync();
        return new Response<List<Job>>(HttpStatusCode.OK, "ok", jobs);

    }

    public async Task<PagedResult<Job>> GetPagedAsync(JobFilter filter,PagedQuery querypage)
    {
        var query = context.Jobs.AsQueryable();
        var skillQuery = context.JobSkills.Join(context.Skills, js => js.SkillId, s => s.Id, (js, s) => new { js.JobId, s.Name });
        if (filter.OrganizationId!=null)
        {
            query = query.Where(j => j.OrganizationId == filter.OrganizationId.Value);
        }
        if (filter.CategoryId!=null)
        {
            query = query.Where(j => j.CategoryId == filter.CategoryId.Value);
        }
        if (filter.Title!=null)
        {
            var pattern = $"%{filter.Title.Trim()}%";
            query = query.Where(j =>
                EF.Functions.ILike(j.Title, pattern) ||
                (j.Description != null && EF.Functions.ILike(j.Description, pattern)) ||
                skillQuery.Any(x => x.JobId == j.Id && EF.Functions.ILike(x.Name, pattern)));
        }
        if (filter.Location!=null)
        {
            var pattern = $"%{filter.Location.Trim()}%";
            query = query.Where(j => j.Location != null && EF.Functions.ILike(j.Location, pattern));
        }
        if (filter.JobType!=null)
        {
            query = query.Where(j => j.JobType == filter.JobType);
        }
        if (filter.ExperienceLevel!=null)
        {
            query = query.Where(j => j.ExperienceLevel == filter.ExperienceLevel);
        }
        if (filter.SalaryMin.HasValue)
        {
            query = query.Where(j => j.SalaryMin >= filter.SalaryMin.Value);
        }
        if (filter.SalaryMax.HasValue)
        {
            query = query.Where(j => j.SalaryMax <= filter.SalaryMax.Value);
        }
        var total = await query.CountAsync();
        var page = querypage.PageNumber > 0 ? querypage.PageNumber : 1;
        var pageSize = querypage.PageSize > 0 ? querypage.PageSize : 10;    
        var jobs = query.Skip((page - 1) * pageSize).Take(pageSize);
        var jobList = await jobs.ToListAsync();

        return new PagedResult<Job>
        {
            Items = jobList,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<Response<List<Job>>> SearchByTitleAsync(string title)
    {
       var term = title?.Trim();
       if (string.IsNullOrWhiteSpace(term))
           return new Response<List<Job>>(HttpStatusCode.BadRequest, "Search text is required");

       var pattern = $"%{term}%";
       var skillQuery = context.JobSkills.Join(context.Skills, js => js.SkillId, s => s.Id, (js, s) => new { js.JobId, s.Name });
       var job = await context.Jobs
           .Where(j =>
               EF.Functions.ILike(j.Title, pattern) ||
               (j.Description != null && EF.Functions.ILike(j.Description, pattern)) ||
               skillQuery.Any(x => x.JobId == j.Id && EF.Functions.ILike(x.Name, pattern)))
           .ToListAsync();

       return job.Count == 0
           ? new Response<List<Job>>(HttpStatusCode.NotFound, "No jobs found for this search", [])
           : new Response<List<Job>>(HttpStatusCode.OK, "ok", job);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateJobDto dto, int actingUserId)
    {
        var update = await context.Jobs.FindAsync(id);
        if (update == null)
        {
            return new Response<string>(HttpStatusCode.NotFound,"Job not found");
        }
        if (!await IsOrgMemberAsync(actingUserId, update.OrganizationId))
            return new Response<string>(HttpStatusCode.Forbidden, "You cannot edit this job");
        update.Title = dto.Title;
        update.Description = dto.Description;
        update.SalaryMin = dto.SalaryMin;
        update.SalaryMax = dto.SalaryMax;
        update.Location = dto.Location;
        update.JobType = dto.JobType;
        update.ExperienceLevel = dto.ExperienceLevel;
        update.ExperienceRequired = dto.ExperienceRequired;
        update.CategoryId = dto.CategoryId;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK,"ok" );
    }

}
