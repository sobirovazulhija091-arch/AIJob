using System.Security.Claims;
using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IGoogleAiService _googleAiService;
    private readonly IAiCareerService _aiCareerService;

    public AiController(IGoogleAiService googleAiService, IAiCareerService aiCareerService)
    {
        _googleAiService = googleAiService;
        _aiCareerService = aiCareerService;
    }

    [HttpPost("ask")]
    [AllowAnonymous]
    public async Task<Response<string>> AskAsync([FromBody] CreateAiPromptDto dto)
    {
        return await _googleAiService.AskAsync(dto);
    }

    [HttpPost("analyze-cv")]
    [Authorize]
    public async Task<Response<AiCvAnalysisResultDto>> AnalyzeCvAsync([FromBody] AiCvAnalysisRequestDto dto)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var userId))
            dto.UserId = userId;

        return await _aiCareerService.AnalyzeCvAsync(dto);
    }

    [HttpGet("skill-gap/{userId}/{jobId}")]
    [Authorize]
    public async Task<Response<AiSkillGapResultDto>> GetSkillGapAsync(int userId, int jobId)
    {
        return await _aiCareerService.GetSkillGapAsync(userId, jobId);
    }

    [HttpPost("improve-job")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<AiJobImproveResultDto>> ImproveJobAsync([FromBody] AiJobImproveRequestDto dto)
    {
        return await _aiCareerService.ImproveJobAsync(dto);
    }

    [HttpPost("draft-cover-letter")]
    [Authorize]
    public async Task<Response<AiDraftResultDto>> DraftCoverLetterAsync([FromBody] AiDraftCoverLetterRequestDto dto)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var uid))
            dto.UserId = uid;

        return await _aiCareerService.DraftCoverLetterAsync(dto);
    }

    [HttpPost("draft-message")]
    [Authorize]
    public async Task<Response<AiDraftResultDto>> DraftMessageAsync([FromBody] AiDraftMessageRequestDto dto)
    {
        return await _aiCareerService.DraftMessageAsync(dto);
    }
}
