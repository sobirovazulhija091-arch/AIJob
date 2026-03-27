using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All methods require login
public class ConnectionController : ControllerBase
{
    private readonly IConnectionService _connectionService;

    public ConnectionController(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    // Gets current user id from JWT token
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Converts service result to HTTP response (404 → NotFound, 403 → Forbidden, 400 → BadRequest)
    private IActionResult ToHttpResult<T>(Response<T> result)
    {
        if (result.StatusCode == 404) return NotFound(result);
        if (result.StatusCode == 403) return StatusCode(403, result);
        if (result.StatusCode == 400) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("send/{addresseeId}")]
    public async Task<IActionResult> SendRequestAsync(int addresseeId)
    {
        var result = await _connectionService.SendRequestAsync(GetUserId(), addresseeId);
        return ToHttpResult(result);
    }

    [HttpPost("send-by-email")]
    public async Task<IActionResult> SendRequestByEmailAsync([FromBody] SendConnectionByEmailDto dto)
    {
        var result = await _connectionService.SendRequestByEmailAsync(GetUserId(), dto.Email);
        return ToHttpResult(result);
    }

    [HttpPut("{connectionId}/respond")]
    public async Task<IActionResult> RespondAsync(int connectionId, [FromBody] UpdateConnectionDto dto)
    {
        var result = await _connectionService.RespondToRequestAsync(connectionId, GetUserId(), dto.Status);
        return ToHttpResult(result);//have all 400 404 so on
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _connectionService.GetByIdAsync(id);
        return ToHttpResult(result);
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

    // Returns all your connections (sent + received, any status) - use to find connection ID
    [HttpGet("all")]
    public async Task<Response<List<Connection>>> GetAllForUserAsync()
    {
        return await _connectionService.GetAllForUserAsync(GetUserId());
    }

    [HttpDelete("{connectionId}")]
    public async Task<IActionResult> RemoveAsync(int connectionId)
    {
        var result = await _connectionService.RemoveConnectionAsync(connectionId, GetUserId());
        return ToHttpResult(result);
    }
}
