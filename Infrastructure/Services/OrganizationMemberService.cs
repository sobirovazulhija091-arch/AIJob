using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class OrganizationMemberService(ApplicationDbContext dbContext, INotificationService notifications) : IOrganizationMemberService
{
    private readonly ApplicationDbContext context = dbContext;
    private readonly INotificationService _notifications = notifications;

    public async Task<Response<string>> CreateAsync(CreateOrganizationMemberDto dto, int actingUserId)
    {
        if (dto.UserId != actingUserId)
            return new Response<string>(HttpStatusCode.BadRequest, "Use the invitation flow to add other users to an organization.");

        var exists = await context.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == dto.OrganizationId && m.UserId == dto.UserId);
        if (exists)
            return new Response<string>(HttpStatusCode.BadRequest, "This user is already a member of this organization");

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

    public async Task<Response<string>> InviteAsync(CreateOrganizationMemberDto dto, int actingUserId)
    {
        if (dto.UserId == actingUserId)
            return new Response<string>(HttpStatusCode.BadRequest, "Invite another user, or add yourself when you create the organization.");

        var inviterIsMember = await context.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == dto.OrganizationId && m.UserId == actingUserId);
        if (!inviterIsMember)
            return new Response<string>(HttpStatusCode.Forbidden, "You are not a member of this organization");

        var inviteeExists = await context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!inviteeExists)
            return new Response<string>(HttpStatusCode.BadRequest, "User not found");

        var alreadyMember = await context.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == dto.OrganizationId && m.UserId == dto.UserId);
        if (alreadyMember)
            return new Response<string>(HttpStatusCode.BadRequest, "This user is already a member of this organization");

        var pending = await context.OrganizationMemberInvitations.AnyAsync(i =>
            i.OrganizationId == dto.OrganizationId &&
            i.InvitedUserId == dto.UserId &&
            i.Status == OrganizationMemberInviteStatus.Pending);
        if (pending)
            return new Response<string>(HttpStatusCode.BadRequest, "An invitation is already pending for this user");

        var org = await context.Organizations.FindAsync(dto.OrganizationId);
        var orgName = org?.Name ?? "an organization";
        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Member" : dto.Role.Trim();

        var invitation = new OrganizationMemberInvitation
        {
            OrganizationId = dto.OrganizationId,
            InvitedUserId = dto.UserId,
            InvitedByUserId = actingUserId,
            Role = role,
            Status = OrganizationMemberInviteStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        await context.OrganizationMemberInvitations.AddAsync(invitation);
        await context.SaveChangesAsync();

        try
        {
            await _notifications.CreateAsync(new CreateNotificationDto
            {
                UserId = dto.UserId,
                Type = NotificationType.OrganizationMemberInvite,
                Title = "Organization invitation",
                Message = $"You were invited to join {orgName} as {role}.",
                RelatedId = invitation.Id,
            });
        }
        catch
        {
        }

        return new Response<string>(HttpStatusCode.OK, "Invitation sent");
    }

    public async Task<Response<string>> RespondToInvitationAsync(int invitationId, int userId, OrganizationMemberInviteRespondDto dto)
    {
        var inv = await context.OrganizationMemberInvitations.FindAsync(invitationId);
        if (inv == null)
            return new Response<string>(HttpStatusCode.NotFound, "Invitation not found");
        if (inv.InvitedUserId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "This invitation is not for you");
        if (inv.Status != OrganizationMemberInviteStatus.Pending)
            return new Response<string>(HttpStatusCode.BadRequest, "This invitation was already answered");

        var status = (OrganizationMemberInviteStatus)dto.Status;
        if (status != OrganizationMemberInviteStatus.Accepted && status != OrganizationMemberInviteStatus.Declined)
            return new Response<string>(HttpStatusCode.BadRequest, "Invalid response");

        if (status == OrganizationMemberInviteStatus.Declined)
        {
            inv.Status = OrganizationMemberInviteStatus.Declined;
            await context.SaveChangesAsync();
            return new Response<string>(HttpStatusCode.OK, "Invitation declined");
        }

        var already = await context.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == inv.OrganizationId && m.UserId == inv.InvitedUserId);
        if (already)
        {
            inv.Status = OrganizationMemberInviteStatus.Accepted;
            await context.SaveChangesAsync();
            return new Response<string>(HttpStatusCode.OK, "Already a member");
        }

        await context.OrganizationMembers.AddAsync(new OrganizationMember
        {
            OrganizationId = inv.OrganizationId,
            UserId = inv.InvitedUserId,
            Role = inv.Role,
        });
        inv.Status = OrganizationMemberInviteStatus.Accepted;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "You joined the organization");
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
