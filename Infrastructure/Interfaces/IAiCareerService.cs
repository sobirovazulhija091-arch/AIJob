using Domain.DTOs;
using Infrastructure.Responses;

public interface IAiCareerService
{
    Task<Response<AiCvAnalysisResultDto>> AnalyzeCvAsync(AiCvAnalysisRequestDto dto);
    Task<Response<AiSkillGapResultDto>> GetSkillGapAsync(int userId, int jobId);
    Task<Response<AiJobImproveResultDto>> ImproveJobAsync(AiJobImproveRequestDto dto);
    Task<Response<AiDraftResultDto>> DraftCoverLetterAsync(AiDraftCoverLetterRequestDto dto);
    Task<Response<AiDraftResultDto>> DraftMessageAsync(AiDraftMessageRequestDto dto);
}
