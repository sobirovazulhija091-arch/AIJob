using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class OrganizationMemberService(ApplicationDbContext dbContext) : IOrganizationMemberService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateOrganizationMemberDto dto)
    {
        var member = new OrganizationMember
        {
            OrganizationId = dto.OrganizationId,
            UserId = dto.UserId,
            Role = dto.Role
        };
        await context.OrganizationMembers.AddAsync(member);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add OrganizationMember successfully");
    }

    public async Task<Response<OrganizationMember>> GetByIdAsync(int id)
    {
        var get = await context.OrganizationMembers.FindAsync(id);
        if (get == null)
            return new Response<OrganizationMember>(HttpStatusCode.NotFound, "OrganizationMember not found");
        return new Response<OrganizationMember>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<OrganizationMember>>> GetAllAsync()
    {
        var list = await context.OrganizationMembers.ToListAsync();
        return new Response<List<OrganizationMember>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateOrganizationMemberDto dto)
    {
        var update = await context.OrganizationMembers.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "OrganizationMember not found");

        update.Role = dto.Role;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.OrganizationMembers.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "OrganizationMember not found");

        context.OrganizationMembers.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted OrganizationMember successfully");
    }

    public async Task<Response<List<OrganizationMember>>> GetByOrganizationIdAsync(int organizationId)
    {
        var list = await context.OrganizationMembers.Where(om => om.OrganizationId == organizationId).ToListAsync();
        return new Response<List<OrganizationMember>>(HttpStatusCode.OK, "ok", list);
    }
}
