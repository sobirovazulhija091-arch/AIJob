using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class UserSkillService(ApplicationDbContext dbContext) : IUserSkillService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateUserSkillDto dto)
    {
        var userSkill = new UserSkill
        {
            UserId = dto.UserId,
            SkillId = dto.SkillId,
            SkillName = dto.SkillName
        };
        await context.UserSkills.AddAsync(userSkill);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add UserSkill successfully");
    }

    public async Task<Response<UserSkill>> GetByIdAsync(int id)
    {
        var get = await context.UserSkills.FindAsync(id);
        if (get == null)
            return new Response<UserSkill>(HttpStatusCode.NotFound, "UserSkill not found");
        return new Response<UserSkill>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<UserSkill>>> GetAllAsync()
    {
        var list = await context.UserSkills.ToListAsync();
        return new Response<List<UserSkill>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateUserSkillDto dto)
    {
        var update = await context.UserSkills.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserSkill not found");

        update.SkillId = dto.SkillId;
        update.SkillName = dto.SkillName;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.UserSkills.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserSkill not found");

        context.UserSkills.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted UserSkill successfully");
    }

    public async Task<Response<List<Skill>>> GetSkillsByUserIdAsync(int userId)
    {
        var userSkills = await context.UserSkills.Where(us => us.UserId == userId).ToListAsync();
        var skillIds = userSkills.Select(us => us.SkillId).Distinct().ToList();
        var skills = await context.Skills.Where(s => skillIds.Contains(s.Id)).ToListAsync();
        return new Response<List<Skill>>(HttpStatusCode.OK, "ok", skills);
    }

    public async Task<Response<string>> RemoveSkillFromUserAsync(int userId, int skillId)
    {
        var userSkill = await context.UserSkills.FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);
        if (userSkill == null)
            return new Response<string>(HttpStatusCode.NotFound, "UserSkill not found");

        context.UserSkills.Remove(userSkill);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Skill removed successfully");
    }
}
