using System.Net;
using System.Security.Claims;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class NotificationService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor) : INotificationService
{
    private readonly ApplicationDbContext context = dbContext;
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    public async Task<Response<string>> CreateAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            RelatedId = dto.RelatedId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await context.Notifications.AddAsync(notification);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add Notification successfully");
    }

    public async Task<Response<Notification>> GetByIdAsync(int id)
    {
        var get = await context.Notifications.FindAsync(id);
        if (get == null)
            return new Response<Notification>(HttpStatusCode.NotFound, "Notification not found");
        return new Response<Notification>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Notification>>> GetAllAsync()
    {
        var list = await context.Notifications.ToListAsync();
        return new Response<List<Notification>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<PagedResult<Notification>> GetPagedAsync(int userId, PagedQuery querypage)
    {
        var query = context.Notifications.Where(n => n.UserId == userId);
        var total = await query.CountAsync();
        var page = querypage.PageNumber > 0 ? querypage.PageNumber : 1;
        var pageSize = querypage.PageSize > 0 ? querypage.PageSize : 10;
        var items = await query.OrderByDescending(n => n.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Notification>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.Notifications.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "Notification not found");

        context.Notifications.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted Notification successfully");
    }

    public async Task<Response<List<Notification>>> GetByUserIdAsync(int userId)
    {
        var list = await context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();
        return new Response<List<Notification>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> MarkAsReadAsync(int id)
    {
        var notification = await context.Notifications.FindAsync(id);
        if (notification == null)
            return new Response<string>(HttpStatusCode.NotFound, "Notification not found");

        var uid = CurrentUserId();
        if (!uid.HasValue || notification.UserId != uid.Value)
            return new Response<string>(HttpStatusCode.Forbidden, "Not your notification");

        notification.IsRead = true;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Marked as read");
    }

    private int? CurrentUserId()
    {
        var id = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var v) ? v : null;
    }
}
