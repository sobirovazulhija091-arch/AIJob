using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class ProfileSkillService(ApplicationDbContext dbContext) : IProfileSkillService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<ProfileSkill>> AddAsync(CreateProfileSkillDto dto)
    {
        var profileSkill = new ProfileSkill
        {
            ProfileId = dto.ProfileId,
            SkillId = dto.SkillId,
            EndorsementsCount = dto.EndorsementsCount
        };
        await context.ProfileSkills.AddAsync(profileSkill);
        await context.SaveChangesAsync();
        return new Response<ProfileSkill>(HttpStatusCode.OK, "Add ProfileSkill successfully", profileSkill);
    }

    public async Task<Response<ProfileSkill>> GetByIdAsync(int id)
    {
        var get = await context.ProfileSkills.FindAsync(id);
        if (get == null)
            return new Response<ProfileSkill>(HttpStatusCode.NotFound, "ProfileSkill not found");
        return new Response<ProfileSkill>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<ProfileSkill>>> GetAllAsync()
    {
        var list = await context.ProfileSkills.ToListAsync();
        return new Response<List<ProfileSkill>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<ProfileSkill>> UpdateAsync(int id, UpdateProfileSkillDto dto)
    {
        var update = await context.ProfileSkills.FindAsync(id);
        if (update == null)
            return new Response<ProfileSkill>(HttpStatusCode.NotFound, "ProfileSkill not found");

        update.SkillId = dto.SkillId;
        update.EndorsementsCount = dto.EndorsementsCount;
        await context.SaveChangesAsync();
        return new Response<ProfileSkill>(HttpStatusCode.OK, "ok", update);
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.ProfileSkills.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "ProfileSkill not found");

        context.ProfileSkills.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted ProfileSkill successfully");
    }

    public async Task<Response<List<ProfileSkill>>> GetByProfileIdAsync(int profileId)
    {
        var list = await context.ProfileSkills.Where(x => x.ProfileId == profileId).ToListAsync();
        return new Response<List<ProfileSkill>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> RemoveSkillAsync(int profileId, int skillId)
    {
        var profileSkill = await context.ProfileSkills
            .FirstOrDefaultAsync(ps => ps.ProfileId == profileId && ps.SkillId == skillId);
        if (profileSkill == null)
            return new Response<string>(HttpStatusCode.NotFound, "ProfileSkill not found");

        context.ProfileSkills.Remove(profileSkill);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Skill removed from profile successfully");
    }
}
