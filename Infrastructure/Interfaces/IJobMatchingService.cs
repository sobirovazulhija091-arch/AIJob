using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobMatchingService
{
    Task<PagedResult<JobWithMatchDto>> GetRecommendedJobsForUserAsync(int userId, JobFilter? filter, PagedQuery query);
    Task<PagedResult<ApplicantWithMatchDto>> GetRecommendedApplicantsForJobAsync(int jobId, PagedQuery query);
    Task<Response<string>> GetMatchExplanationAsync(int userId, int jobId, bool useAi = false);
}
