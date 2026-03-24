using System.Net;
using Domain.DTOs;
using Google.GenAI;
using Infrastructure.Responses;
using Microsoft.Extensions.Configuration;

public class GoogleAiService(IConfiguration configuration) : IGoogleAiService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<Response<string>> AskAsync(CreateAiPromptDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt))
            return new Response<string>(HttpStatusCode.BadRequest, "Prompt is required");

        var apiKey = _configuration["Gemini:ApiKey"] ?? _configuration["GoogleAi:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return new Response<string>(HttpStatusCode.BadRequest, "Gemini:ApiKey or GoogleAi:ApiKey is missing in configuration");

        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

        try
        {
            var client = new Client(apiKey: apiKey);
            var response = await client.Models.GenerateContentAsync(model: model, contents: dto.Prompt.Trim());

            var text = response?.Candidates?[0]?.Content?.Parts?[0]?.Text;
            if (string.IsNullOrWhiteSpace(text))
                return new Response<string>(HttpStatusCode.BadRequest, "Empty response from Google AI");

            return new Response<string>(HttpStatusCode.OK, "ok", text);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (msg.Contains("API key not valid") || msg.Contains("API_KEY_INVALID"))
                return new Response<string>(HttpStatusCode.BadRequest, "API key not valid. Get a new key from https://aistudio.google.com/apikey");
            if (msg.Contains("quota") || msg.Contains("Quota exceeded") || msg.Contains("RESOURCE_EXHAUSTED"))
                return new Response<string>(HttpStatusCode.TooManyRequests, "Quota exceeded. Wait 1–2 minutes and retry. Free tier has limits. See https://ai.google.dev/gemini-api/docs/rate-limits");
            return new Response<string>(HttpStatusCode.BadRequest, $"Google AI error: {msg}");
        }
    }
}
