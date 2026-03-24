using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IGoogleAiService _googleAiService;

    public AiController(IGoogleAiService googleAiService)
    {
        _googleAiService = googleAiService;
    }

    [HttpPost("ask")]
    [AllowAnonymous]
    public async Task<Response<string>> AskAsync([FromBody] CreateAiPromptDto dto)
    {
        return await _googleAiService.AskAsync(dto);
    }
}
