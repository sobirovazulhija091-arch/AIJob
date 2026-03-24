using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class JobMatchingController : ControllerBase
{
    private readonly IJobMatchingService _matchingService;

    public JobMatchingController(IJobMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    /// <summary>Get jobs recommended for a candidate, sorted by match score.</summary>
    [HttpGet("recommended-jobs/{userId}")]
    [Authorize]
    public async Task<PagedResult<JobWithMatchDto>> GetRecommendedJobsForUser(
        int userId,
        [FromQuery] JobFilter? filter,
        [FromQuery] PagedQuery query)
    {
        return await _matchingService.GetRecommendedJobsForUserAsync(userId, filter, query);
    }

    /// <summary>Get applicants recommended for a job (for companies), sorted by match score.</summary>
    [HttpGet("recommended-applicants/{jobId}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<PagedResult<ApplicantWithMatchDto>> GetRecommendedApplicantsForJob(
        int jobId,
        [FromQuery] PagedQuery query)
    {
        return await _matchingService.GetRecommendedApplicantsForJobAsync(jobId, query);
    }

    /// <summary>Get match explanation for a user-job pair. Set useAi=true for Gemini-generated insight.</summary>
    [HttpGet("match-explanation/{userId}/{jobId}")]
    [Authorize]
    public async Task<Response<string>> GetMatchExplanation(int userId, int jobId, [FromQuery] bool useAi = false)
    {
        return await _matchingService.GetMatchExplanationAsync(userId, jobId, useAi);
    }
}
