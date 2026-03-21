using System.Net;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class EndorsementService(ApplicationDbContext dbContext) : IEndorsementService
{
    private readonly ApplicationDbContext context = dbContext;
//
    private static async Task<bool> AreConnectedAsync(ApplicationDbContext ctx, int userId1, int userId2)
    {
        return await ctx.Connections.AnyAsync(c =>
            ((c.RequesterId == userId1 && c.AddresseeId == userId2) ||
             (c.RequesterId == userId2 && c.AddresseeId == userId1)) &&
            c.Status == ConnectionStatus.Accepted);
    }

    public async Task<Response<string>> AddAsync(int endorserId, CreateEndorsementDto dto)
    {
        var profileSkill = await context.ProfileSkills.FindAsync(dto.ProfileSkillId);
        if (profileSkill == null)
            return new Response<string>(HttpStatusCode.NotFound, "Profile skill not found");

        var profile = await context.Profiles.FindAsync(profileSkill.ProfileId);
        if (profile == null)
            return new Response<string>(HttpStatusCode.NotFound, "Profile not found");

        var endorsedUserId = profile.UserId;
        if (endorserId == endorsedUserId)
            return new Response<string>(HttpStatusCode.BadRequest, "Cannot endorse your own skill");

        var connected = await AreConnectedAsync(context, endorserId, endorsedUserId);
        if (!connected)
            return new Response<string>(HttpStatusCode.Forbidden, "Must be connected to endorse");

        var alreadyEndorsed = await context.Endorsements
            .AnyAsync(e => e.EndorserId == endorserId && e.ProfileSkillId == dto.ProfileSkillId);
        if (alreadyEndorsed)
            return new Response<string>(HttpStatusCode.BadRequest, "Already endorsed this skill");

        var endorsement = new Endorsement
        {
            EndorserId = endorserId,
            ProfileSkillId = dto.ProfileSkillId,
            CreatedAt = DateTime.UtcNow
        };
        await context.Endorsements.AddAsync(endorsement);
        profileSkill.EndorsementsCount++;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Endorsement added");
    }

    public async Task<Response<string>> RemoveAsync(int endorsementId, int userId)
    {
        var endorsement = await context.Endorsements.FindAsync(endorsementId);
        if (endorsement == null)
            return new Response<string>(HttpStatusCode.NotFound, "Endorsement not found");
        if (endorsement.EndorserId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "Not your endorsement");

        var profileSkill = await context.ProfileSkills.FindAsync(endorsement.ProfileSkillId);
        context.Endorsements.Remove(endorsement);
        if (profileSkill != null)
            profileSkill.EndorsementsCount = Math.Max(0, profileSkill.EndorsementsCount - 1);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Endorsement removed");
    }

    public async Task<Response<Endorsement>> GetByIdAsync(int id)
    {
        var e = await context.Endorsements.FindAsync(id);
        if (e == null)
            return new Response<Endorsement>(HttpStatusCode.NotFound, "Endorsement not found");
        return new Response<Endorsement>(HttpStatusCode.OK, "ok", e);
    }

    public async Task<Response<List<Endorsement>>> GetByProfileSkillIdAsync(int profileSkillId)
    {
        var list = await context.Endorsements
            .Where(e => e.ProfileSkillId == profileSkillId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
        return new Response<List<Endorsement>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<List<Endorsement>>> GetByUserIdAsync(int userId)
    {
        var list = await context.Endorsements
            .Where(e => e.EndorserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
        return new Response<List<Endorsement>>(HttpStatusCode.OK, "ok", list);
    }
}
