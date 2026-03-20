using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> CreateAsync([FromBody] CreatePostDto dto)
    {
        return await _postService.CreateAsync(GetUserId(), dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<Post>> GetByIdAsync(int id)
    {
        return await _postService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<Post>>> GetAllAsync()
    {
        return await _postService.GetAllAsync();
    }

    [HttpGet("feed")]
    [Authorize]
    public async Task<Response<List<Post>>> GetFeedAsync()
    {
        return await _postService.GetFeedAsync(GetUserId());
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, [FromBody] UpdatePostDto dto)
    {
        return await _postService.UpdateAsync(id, GetUserId(), dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _postService.DeleteAsync(id, GetUserId());
    }
}
