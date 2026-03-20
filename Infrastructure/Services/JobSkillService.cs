using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class JobSkillService(ApplicationDbContext dbContext) : IJobSkillService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateJobSkillDto dto)
    {
        var jobSkill = new JobSkill
        {
            JobId = dto.JobId,
            SkillId = dto.SkillId
        };
        await context.JobSkills.AddAsync(jobSkill);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add JobSkill successfully");
    }

    public async Task<Response<JobSkill>> GetByIdAsync(int id)
    {
        var get = await context.JobSkills.FindAsync(id);
        if (get == null)
            return new Response<JobSkill>(HttpStatusCode.NotFound, "JobSkill not found");
        return new Response<JobSkill>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<JobSkill>>> GetAllAsync()
    {
        var list = await context.JobSkills.ToListAsync();
        return new Response<List<JobSkill>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateJobSkillDto dto)
    {
        var update = await context.JobSkills.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobSkill not found");

        update.JobId = dto.JobId;
        update.SkillId = dto.SkillId;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.JobSkills.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "JobSkill not found");

        context.JobSkills.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted JobSkill successfully");
    }

    public async Task<Response<List<Skill>>> GetSkillsByJobIdAsync(int jobId)
    {
        var jobSkills = await context.JobSkills.Where(js => js.JobId == jobId).ToListAsync();
        var skillIds = jobSkills.Select(js => js.SkillId).Distinct().ToList();
        var skills = await context.Skills.Where(s => skillIds.Contains(s.Id)).ToListAsync();
        return new Response<List<Skill>>(HttpStatusCode.OK, "ok", skills);
    }
}
