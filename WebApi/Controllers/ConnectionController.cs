using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConnectionController : ControllerBase
{
    private readonly IConnectionService _connectionService;

    public ConnectionController(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("send/{addresseeId}")]
    public async Task<Response<string>> SendRequestAsync(int addresseeId)
    {
        return await _connectionService.SendRequestAsync(GetUserId(), addresseeId);
    }

    [HttpPut("{connectionId}/respond")]
    public async Task<Response<string>> RespondAsync(int connectionId, [FromBody] UpdateConnectionDto dto)
    {
        return await _connectionService.RespondToRequestAsync(connectionId, GetUserId(), dto.Status);
    }

    [HttpGet("{id}")]
    public async Task<Response<Connection>> GetByIdAsync(int id)
    {
        return await _connectionService.GetByIdAsync(id);
    }

    [HttpGet("my")]
    public async Task<Response<List<Connection>>> GetMyConnectionsAsync()
    {
        return await _connectionService.GetByUserIdAsync(GetUserId());
    }

    [HttpGet("pending")]
    public async Task<Response<List<Connection>>> GetPendingRequestsAsync()
    {
        return await _connectionService.GetPendingRequestsAsync(GetUserId());
    }

    [HttpDelete("{connectionId}")]
    public async Task<Response<string>> RemoveAsync(int connectionId)
    {
        return await _connectionService.RemoveConnectionAsync(connectionId, GetUserId());
    }
}
