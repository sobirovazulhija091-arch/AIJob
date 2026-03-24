using Domain.DTOs;
using Infrastructure.Responses;

public interface IGoogleAiService
{
    Task<Response<string>> AskAsync(CreateAiPromptDto dto);
}