using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class SkillService(ApplicationDbContext dbContext) : ISkillService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateSkillDto dto)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return new Response<string>(HttpStatusCode.BadRequest, "Skill name is required");

        var exists = await context.Skills.AnyAsync(x => x.Name.ToLower() == name.ToLower());
        if (exists)
            return new Response<string>(HttpStatusCode.BadRequest, "Skill already exists. You cannot add the same skill twice.");

        var skill = new Skill { Name = name, Description = dto.Description };
        await context.Skills.AddAsync(skill);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add Skill successfully");
    }

    public async Task<Response<Skill>> GetByIdAsync(int id)
    {
        var get = await context.Skills.FindAsync(id);
        if (get == null)
            return new Response<Skill>(HttpStatusCode.NotFound, "Skill not found");
        return new Response<Skill>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Skill>>> GetAllAsync()
    {
        var list = await context.Skills.ToListAsync();
        return new Response<List<Skill>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateSkillResponseDto dto)
    {
        var update = await context.Skills.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "Skill not found");

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return new Response<string>(HttpStatusCode.BadRequest, "Skill name is required");

        var exists = await context.Skills.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower());
        if (exists)
            return new Response<string>(HttpStatusCode.BadRequest, "Skill already exists. You cannot add the same skill twice.");

        update.Name = name;
        update.Description = dto.Description;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.Skills.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "Skill not found");

        context.Skills.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted Skill successfully");
    }

    public async Task<Response<List<Skill>>> SearchByNameAsync(string name)
    {
        var term = name?.Trim();
        if (string.IsNullOrWhiteSpace(term))
            return new Response<List<Skill>>(HttpStatusCode.BadRequest, "Search text is required");

        var pattern = $"%{term}%";
        var list = await context.Skills
            .Where(s => EF.Functions.ILike(s.Name, pattern) || EF.Functions.ILike(s.Description, pattern))
            .ToListAsync();

        return list.Count == 0
            ? new Response<List<Skill>>(HttpStatusCode.NotFound, "No skills found for this search", [])
            : new Response<List<Skill>>(HttpStatusCode.OK, "ok", list);
    }
}
