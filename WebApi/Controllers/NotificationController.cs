using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> AddAsync(CreateNotificationDto dto)
    {
        return await _notificationService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<Notification>> GetByIdAsync(int id)
    {
        return await _notificationService.GetByIdAsync(id);
    }

    [HttpGet("paged")]
    [Authorize]
    public async Task<PagedResult<Notification>> GetPagedAsync([FromQuery] int userId, [FromQuery] PagedQuery querypage)
    {
        return await _notificationService.GetPagedAsync(userId, querypage);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _notificationService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<List<Notification>>> GetByUserIdAsync(int userId)
    {
        return await _notificationService.GetByUserIdAsync(userId);
    }

    [HttpPatch("{id}/read")]
    [Authorize]
    public async Task<Response<string>> MarkAsReadAsync(int id)
    {
        return await _notificationService.MarkAsReadAsync(id);
    }
}
