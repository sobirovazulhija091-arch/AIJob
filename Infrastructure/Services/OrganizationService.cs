using System.Net;
using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class OrganizationService(ApplicationDbContext dbContext) : IOrganizationService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<Organization>> CreateAsync(CreateOrganizationDto dto)
    {
        var org = new Organization
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = dto.Location,
            LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl.Trim(),
            Type = Enum.TryParse<OrganizationType>(dto.Type, out var t) ? t : OrganizationType.Company
        };
        await context.Organizations.AddAsync(org);
        await context.SaveChangesAsync();
        return new Response<Organization>(HttpStatusCode.OK, "ok", org);
    }

    public async Task<Response<Organization>> GetByIdAsync(int id)
    {
        var get = await context.Organizations.FindAsync(id);
        if (get == null)
            return new Response<Organization>(HttpStatusCode.NotFound, "Organization not found");
        return new Response<Organization>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Organization>>> GetAllAsync()
    {
        var list = await context.Organizations.ToListAsync();
        return new Response<List<Organization>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<List<Organization>>> GetForUserAsync(int userId)
    {
        var orgIds = await context.OrganizationMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.OrganizationId)
            .Distinct()
            .ToListAsync();
        if (orgIds.Count == 0)
            return new Response<List<Organization>>(HttpStatusCode.OK, "ok", []);
        var list = await context.Organizations.Where(o => orgIds.Contains(o.Id)).ToListAsync();
        return new Response<List<Organization>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<PagedResult<Organization>> GetPagedAsync(OrganizationFilter filter, PagedQuery querypage)
    {
        var query = context.Organizations.AsQueryable();
        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(o => o.Name.Contains(filter.Name));

        var total = await query.CountAsync();
        var page = querypage.PageNumber > 0 ? querypage.PageNumber : 1;
        var pageSize = querypage.PageSize > 0 ? querypage.PageSize : 10;
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Organization>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateOrganizationDto dto, int actingUserId)
    {
        var update = await context.Organizations.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "Organization not found");

        var isMember = await context.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == id && m.UserId == actingUserId);
        if (!isMember)
            return new Response<string>(HttpStatusCode.Forbidden, "You are not a member of this organization");

        update.Name = dto.Name;
        update.Description = dto.Description;
        update.Location = dto.Location;
        update.LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl.Trim();
        if (!string.IsNullOrEmpty(dto.Type) && Enum.TryParse<OrganizationType>(dto.Type, out var t))
            update.Type = t;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id, int actingUserId)
    {
        var del = await context.Organizations.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "Organization not found");

        var isMember = await context.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == id && m.UserId == actingUserId);
        if (!isMember)
            return new Response<string>(HttpStatusCode.Forbidden, "You are not a member of this organization");

        context.Organizations.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted Organization successfully");
    }

    public async Task<Response<List<Organization>>> SearchByNameAsync(string name)
    {
        var list = await context.Organizations.Where(o => o.Name.Contains(name)).ToListAsync();
        return new Response<List<Organization>>(HttpStatusCode.OK, "ok", list);
    }
}
