using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

public class AiCareerService(
    ApplicationDbContext dbContext,
    IGoogleAiService googleAiService,
    IWebHostEnvironment environment) : IAiCareerService
{
    private readonly ApplicationDbContext context = dbContext;
    private readonly IGoogleAiService aiService = googleAiService;
    private readonly IWebHostEnvironment env = environment;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response<AiCvAnalysisResultDto>> AnalyzeCvAsync(AiCvAnalysisRequestDto dto)
    {
        var cvText = await ResolveCvTextAsync(dto.CvText, dto.CvFileUrl);
        if (string.IsNullOrWhiteSpace(cvText))
            return new Response<AiCvAnalysisResultDto>(HttpStatusCode.BadRequest, "CV text could not be extracted. Send CvText or a valid uploaded CV url.");

        var trimmedText = TrimLength(cvText, 12000);
        var prompt = $"""
Return valid JSON only.
Analyze this CV/resume text and extract:
- fullName, firstName, lastName (split name when possible)
- professionalSummary
- experienceYears (integer; use 0 only if employment history is missing, unclear, or you cannot estimate from dates — do not guess years without evidence)
- skills (array of short skill names)
- education (array)
- recommendedRoles (array of concise job titles)
- notes (array: brief quality observations, not duplicates of other fields)
- missingOrWeakSections (array): factual gaps in the document. Examples: "No work experience or employers listed", "Employment dates missing — cannot verify years of experience", "No education section", "No skills or technologies mentioned", "Very short document — looks incomplete", "No measurable achievements or metrics". Be direct; say what is NOT there or too weak.
- howToImprove (array): specific actions the candidate should take. Examples: "Add a reverse-chronological work history with company, role, dates, and 2–4 bullet achievements per role", "State total years of experience or list date ranges so employers can see tenure", "Add a skills section aligned to your target roles", "Quantify results (%, revenue, scale, timelines)".
- helpfulResources (array): 4 to 6 strings in the format "Title — https://full-url" pointing to reputable free guidance (resume writing, examples, or career advice). Prefer well-known sites such as Indeed, LinkedIn, Coursera career articles, Novorésumé, or government/university career centres. Use real https URLs only.

Rules:
- Keep professionalSummary to 2–6 short paragraphs max (not 30 sentences).
- skills: short names only.
- Do not invent employers, degrees, or years not supported by the text. If experience is unclear, experienceYears should be 0 and missingOrWeakSections must explain that.
- If something is missing, use empty string or empty array — never null.
- Separate concerns: notes = quick observations; missingOrWeakSections = what's absent/weak; howToImprove = what to do next.
CV TEXT:
{trimmedText}
""";

        var aiJson = await AskForJsonAsync<AiCvAnalysisJsonDto>(prompt);
        var result = aiJson != null
            ? new AiCvAnalysisResultDto
            {
                FullName = aiJson.FullName?.Trim() ?? "",
                FirstName = aiJson.FirstName?.Trim() ?? "",
                LastName = aiJson.LastName?.Trim() ?? "",
                ProfessionalSummary = aiJson.ProfessionalSummary?.Trim() ?? "",
                ExperienceYears = Math.Max(0, aiJson.ExperienceYears ?? 0),
                Skills = NormalizeDistinct(aiJson.Skills),
                Education = NormalizeDistinct(aiJson.Education),
                RecommendedRoles = NormalizeDistinct(aiJson.RecommendedRoles),
                Notes = NormalizeDistinct(aiJson.Notes),
                MissingOrWeakSections = NormalizeDistinct(aiJson.MissingOrWeakSections),
                HowToImprove = NormalizeDistinct(aiJson.HowToImprove),
                HelpfulResources = NormalizeDistinct(aiJson.HelpfulResources),
                SourceTextPreview = TrimLength(trimmedText, 500)
            }
            : BuildFallbackCvAnalysis(trimmedText);

        if (dto.UserId.HasValue && dto.UserId.Value > 0 && (dto.ApplyToProfile || dto.SyncSkills))
            await ApplyCvAnalysisAsync(dto.UserId.Value, dto.CvFileUrl, result, dto.ApplyToProfile, dto.SyncSkills);

        return new Response<AiCvAnalysisResultDto>(HttpStatusCode.OK, "ok", result);
    }

    public async Task<Response<AiSkillGapResultDto>> GetSkillGapAsync(int userId, int jobId)
    {
        var job = await context.Jobs.FindAsync(jobId);
        if (job == null)
            return new Response<AiSkillGapResultDto>(HttpStatusCode.NotFound, "Job not found");

        var userSkillNames = await GetUserSkillNamesAsync(userId);
        var jobSkillNames = await GetJobSkillNamesAsync(jobId);
        var userExpYears = await GetUserExperienceYearsAsync(userId);
        var matchedSkillNames = jobSkillNames.Intersect(userSkillNames, StringComparer.OrdinalIgnoreCase).ToList();
        var missingSkillNames = jobSkillNames.Except(userSkillNames, StringComparer.OrdinalIgnoreCase).ToList();
        var skillScore = jobSkillNames.Count == 0 ? 60 : (int)Math.Round((double)matchedSkillNames.Count / jobSkillNames.Count * 60);
        var expScore = job.ExperienceRequired <= 0
            ? 40
            : userExpYears >= job.ExperienceRequired
                ? 40
                : (int)Math.Round((double)userExpYears / job.ExperienceRequired * 40);
        var total = Math.Min(100, skillScore + expScore);

        var fallback = new AiSkillGapResultDto
        {
            MatchScore = total,
            FitSummary = BuildFallbackSkillGapSummary(job.Title, total, matchedSkillNames, missingSkillNames, userExpYears, job.ExperienceRequired),
            Strengths = matchedSkillNames.Count > 0 ? matchedSkillNames : ["Build out your profile skills to improve matching"],
            MissingSkills = missingSkillNames,
            NextSteps = BuildNextSteps(missingSkillNames, userExpYears, job.ExperienceRequired)
        };

        var prompt = $"""
Return valid JSON only.
You are helping a job candidate understand their skill gap.
Job title: {job.Title}
Job required skills: {JoinOrNone(jobSkillNames)}
Candidate skills: {JoinOrNone(userSkillNames)}
Candidate experience years: {userExpYears}
Job required experience years: {job.ExperienceRequired}
Current match score: {total}/100

Return fields:
- fitSummary
- strengths (array)
- missingSkills (array)
- nextSteps (array)

Keep the advice practical and concise.
""";

        var aiJson = await AskForJsonAsync<AiSkillGapJsonDto>(prompt);
        if (aiJson != null)
        {
            fallback.FitSummary = string.IsNullOrWhiteSpace(aiJson.FitSummary) ? fallback.FitSummary : aiJson.FitSummary.Trim();
            var strengths = NormalizeDistinct(aiJson.Strengths);
            if (strengths.Count > 0) fallback.Strengths = strengths;
            var missing = NormalizeDistinct(aiJson.MissingSkills);
            if (missing.Count > 0) fallback.MissingSkills = missing;
            var nextSteps = NormalizeDistinct(aiJson.NextSteps);
            if (nextSteps.Count > 0) fallback.NextSteps = nextSteps;
        }

        return new Response<AiSkillGapResultDto>(HttpStatusCode.OK, "ok", fallback);
    }

    public async Task<Response<AiJobImproveResultDto>> ImproveJobAsync(AiJobImproveRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) && !dto.JobId.HasValue)
            return new Response<AiJobImproveResultDto>(HttpStatusCode.BadRequest, "Title or JobId is required");

        Job? job = null;
        if (dto.JobId.HasValue)
        {
            job = await context.Jobs.FindAsync(dto.JobId.Value);
            if (job == null)
                return new Response<AiJobImproveResultDto>(HttpStatusCode.NotFound, "Job not found");
        }

        var title = string.IsNullOrWhiteSpace(dto.Title) ? job?.Title ?? "" : dto.Title.Trim();
        var description = string.IsNullOrWhiteSpace(dto.Description) ? job?.Description ?? "" : dto.Description.Trim();
        var location = string.IsNullOrWhiteSpace(dto.Location) ? job?.Location ?? "" : dto.Location.Trim();
        var experienceRequired = dto.ExperienceRequired > 0 ? dto.ExperienceRequired : job?.ExperienceRequired ?? 0;

        var prompt = $"""
Return valid JSON only.
Improve this job post for a professional job platform.

Title: {title}
Location: {location}
Experience required: {experienceRequired}
Description:
{description}

Return fields:
- improvedTitle
- improvedDescription
- suggestedSkills (array)
- suggestedResponsibilities (array)
- suggestedBenefits (array)

Rules:
- Keep improvedDescription under 1800 characters.
- Make it clear, professional, and practical.
- Do not invent impossible requirements.
- suggestedSkills should be concise skill names relevant to the job.
- suggestedResponsibilities should be concise and specific.
- suggestedBenefits should be concise and appealing.
- If something is missing, return empty string or empty array.
""";

        var aiJson = await AskForJsonAsync<AiJobImproveJsonDto>(prompt);
        var result = new AiJobImproveResultDto
        {
            ImprovedTitle = aiJson?.ImprovedTitle?.Trim() ?? title,
            ImprovedDescription = aiJson?.ImprovedDescription?.Trim() ?? description,
            SuggestedSkills = NormalizeDistinct(aiJson?.SuggestedSkills),
            SuggestedResponsibilities = NormalizeDistinct(aiJson?.SuggestedResponsibilities),
            SuggestedBenefits = NormalizeDistinct(aiJson?.SuggestedBenefits)
        };

        if (dto.ApplyToJob && job != null)
        {
            job.Title = string.IsNullOrWhiteSpace(result.ImprovedTitle) ? job.Title : result.ImprovedTitle;
            job.Description = string.IsNullOrWhiteSpace(result.ImprovedDescription) ? job.Description : TrimLength(result.ImprovedDescription, 4000);
            await context.SaveChangesAsync();
        }

        return new Response<AiJobImproveResultDto>(HttpStatusCode.OK, "ok", result);
    }

    public async Task<Response<AiDraftResultDto>> DraftCoverLetterAsync(AiDraftCoverLetterRequestDto dto)
    {
        var job = await context.Jobs.FindAsync(dto.JobId);
        if (job == null)
            return new Response<AiDraftResultDto>(HttpStatusCode.NotFound, "Job not found");

        var user = await context.Users.FindAsync(dto.UserId);
        if (user == null)
            return new Response<AiDraftResultDto>(HttpStatusCode.NotFound, "User not found");

        var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == dto.UserId);
        var skillNames = await GetUserSkillNamesAsync(dto.UserId);
        var prompt = $"""
Return valid JSON only.
Draft a concise cover letter for this candidate.

Candidate name: {user.FullName}
Candidate summary: {profile?.AboutMe ?? ""}
Candidate experience years: {profile?.ExperienceYears ?? await GetUserExperienceYearsAsync(dto.UserId)}
Candidate skills: {JoinOrNone(skillNames)}
Job title: {job.Title}
Job description: {job.Description ?? ""}
Tone: {dto.Tone ?? "professional"}
Extra context: {dto.ExtraContext ?? ""}

Return fields:
- subject
- content

Keep content to 3-5 short paragraphs and make it realistic.
""";

        var aiJson = await AskForJsonAsync<AiDraftJsonDto>(prompt);
        var subject = aiJson?.Subject?.Trim();
        var content = aiJson?.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            subject = $"Application for {job.Title}";
            content = BuildFallbackCoverLetter(user.FullName, job.Title, profile?.AboutMe, skillNames, dto.ExtraContext);
        }

        return new Response<AiDraftResultDto>(HttpStatusCode.OK, "ok", new AiDraftResultDto
        {
            Subject = string.IsNullOrWhiteSpace(subject) ? $"Application for {job.Title}" : subject,
            Content = content
        });
    }

    public async Task<Response<AiDraftResultDto>> DraftMessageAsync(AiDraftMessageRequestDto dto)
    {
        var user = await context.Users.FindAsync(dto.UserId);
        if (user == null)
            return new Response<AiDraftResultDto>(HttpStatusCode.NotFound, "User not found");

        Job? job = null;
        if (dto.JobId.HasValue)
            job = await context.Jobs.FindAsync(dto.JobId.Value);

        var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == dto.UserId);
        var prompt = $"""
Return valid JSON only.
Draft a professional message.

Sender name: {user.FullName}
Recipient name: {dto.RecipientName}
Purpose: {dto.Purpose}
Related job title: {job?.Title ?? ""}
Sender summary: {profile?.AboutMe ?? ""}
Tone: {dto.Tone ?? "professional"}
Extra context: {dto.ExtraContext ?? ""}

Return fields:
- subject
- content

Keep it short, warm, and action-oriented.
""";

        var aiJson = await AskForJsonAsync<AiDraftJsonDto>(prompt);
        var subject = aiJson?.Subject?.Trim();
        var content = aiJson?.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            subject = string.IsNullOrWhiteSpace(subject) ? dto.Purpose : subject;
            content = BuildFallbackMessage(dto.RecipientName, dto.Purpose, dto.ExtraContext);
        }

        return new Response<AiDraftResultDto>(HttpStatusCode.OK, "ok", new AiDraftResultDto
        {
            Subject = string.IsNullOrWhiteSpace(subject) ? dto.Purpose : subject,
            Content = content
        });
    }

    private async Task<T?> AskForJsonAsync<T>(string prompt) where T : class
    {
        var response = await aiService.AskAsync(new CreateAiPromptDto { Prompt = prompt });
        if (response.StatusCode != (int)HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Data))
            return null;

        var cleaned = ExtractJson(response.Data);
        if (string.IsNullOrWhiteSpace(cleaned))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task ApplyCvAnalysisAsync(int userId, string? cvFileUrl, AiCvAnalysisResultDto result, bool applyToProfile, bool syncSkills)
    {
        if (applyToProfile)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    AboutMe = result.ProfessionalSummary,
                    ExperienceYears = result.ExperienceYears,
                    ExpectedSalary = 0,
                    CVFileUrl = cvFileUrl
                };
                await context.UserProfiles.AddAsync(profile);
            }
            else
            {
                profile.FirstName = string.IsNullOrWhiteSpace(result.FirstName) ? profile.FirstName : result.FirstName;
                profile.LastName = string.IsNullOrWhiteSpace(result.LastName) ? profile.LastName : result.LastName;
                profile.AboutMe = string.IsNullOrWhiteSpace(result.ProfessionalSummary) ? profile.AboutMe : result.ProfessionalSummary;
                profile.ExperienceYears = result.ExperienceYears > 0 ? result.ExperienceYears : profile.ExperienceYears;
                profile.CVFileUrl = string.IsNullOrWhiteSpace(cvFileUrl) ? profile.CVFileUrl : cvFileUrl;
            }
        }

        if (syncSkills && result.Skills.Count > 0)
            await SyncUserSkillsAsync(userId, result.Skills);

        await context.SaveChangesAsync();
    }

    private async Task SyncUserSkillsAsync(int userId, List<string> skillNames)
    {
        var normalizedNames = NormalizeDistinct(skillNames);
        if (normalizedNames.Count == 0) return;

        var existingSkills = await context.Skills.ToListAsync();
        var existingUserSkills = await context.UserSkills.Where(x => x.UserId == userId).ToListAsync();

        foreach (var skillName in normalizedNames)
        {
            var skill = existingSkills.FirstOrDefault(x => x.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill == null)
            {
                skill = new Skill
                {
                    Name = skillName,
                    Description = $"Added from AI CV analysis for {skillName}"
                };
                await context.Skills.AddAsync(skill);
                existingSkills.Add(skill);
                await context.SaveChangesAsync();
            }

            if (existingUserSkills.Any(x => x.SkillId == skill.Id)) continue;
            var link = new UserSkill
            {
                UserId = userId,
                SkillId = skill.Id,
                SkillName = skill.Name
            };
            await context.UserSkills.AddAsync(link);
            existingUserSkills.Add(link);
        }
    }

    private async Task<List<string>> GetUserSkillNamesAsync(int userId)
    {
        var userSkillNames = await context.UserSkills.Where(x => x.UserId == userId).Select(x => x.SkillName).ToListAsync();
        var profile = await context.Profiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null)
            return NormalizeDistinct(userSkillNames);

        var profileSkillNames = await context.ProfileSkills
            .Where(x => x.ProfileId == profile.Id)
            .Join(context.Skills, x => x.SkillId, x => x.Id, (ps, s) => s.Name)
            .ToListAsync();

        return NormalizeDistinct(userSkillNames.Concat(profileSkillNames).ToList());
    }

    private async Task<List<string>> GetJobSkillNamesAsync(int jobId)
    {
        return NormalizeDistinct(await context.JobSkills
            .Where(x => x.JobId == jobId)
            .Join(context.Skills, x => x.SkillId, x => x.Id, (js, s) => s.Name)
            .ToListAsync());
    }

    private async Task<int> GetUserExperienceYearsAsync(int userId)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile != null && profile.ExperienceYears > 0)
            return profile.ExperienceYears;

        var experiences = await context.UserExperiences.Where(x => x.UserId == userId).ToListAsync();
        if (experiences.Count == 0)
            return 0;

        var totalMonths = experiences.Sum(x => (int)Math.Max(0, (x.EndDate - x.StartDate).TotalDays / 30));
        return totalMonths / 12;
    }

    private async Task<string> ResolveCvTextAsync(string? cvText, string? cvFileUrl)
    {
        if (!string.IsNullOrWhiteSpace(cvText))
            return cvText.Trim();
        if (string.IsNullOrWhiteSpace(cvFileUrl))
            return "";

        var filePath = ResolveCvPath(cvFileUrl);
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return "";

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".docx" => await ExtractDocxTextAsync(filePath),
            ".txt" => await File.ReadAllTextAsync(filePath),
            ".pdf" => await ExtractBinaryTextAsync(filePath),
            ".doc" => await ExtractBinaryTextAsync(filePath),
            _ => await ExtractBinaryTextAsync(filePath)
        };
    }

    private string ResolveCvPath(string cvFileUrl)
    {
        var relative = cvFileUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        return Path.Combine(root, relative);
    }

    private static async Task<string> ExtractDocxTextAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
        var entry = archive.GetEntry("word/document.xml");
        if (entry == null) return "";
        using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream);
        var xml = await reader.ReadToEndAsync();
        var text = Regex.Replace(xml, "<[^>]+>", " ");
        return NormalizeWhitespace(System.Net.WebUtility.HtmlDecode(text));
    }

    private static async Task<string> ExtractBinaryTextAsync(string filePath)
    {
        var bytes = await File.ReadAllBytesAsync(filePath);
        var raw = Encoding.UTF8.GetString(bytes);
        var candidates = Regex.Matches(raw, @"[\u0020-\u007E]{4,}").Select(x => x.Value).ToList();
        return NormalizeWhitespace(string.Join(" ", candidates));
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            trimmed = Regex.Replace(trimmed, @"^```(?:json)?", "", RegexOptions.IgnoreCase).Trim();
            trimmed = Regex.Replace(trimmed, @"```$", "").Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        return start >= 0 && end > start ? trimmed[start..(end + 1)] : trimmed;
    }

    private static AiCvAnalysisResultDto BuildFallbackCvAnalysis(string cvText)
    {
        var preview = TrimLength(cvText, 500);
        var weak = new List<string>();
        if (string.IsNullOrWhiteSpace(cvText) || cvText.Length < 200)
            weak.Add("The extracted text is very short — the file may be hard to read, or the CV is incomplete.");
        if (!Regex.IsMatch(cvText, @"20\d{2}|19\d{2}"))
            weak.Add("No clear years or date ranges detected — add employment and education dates.");
        if (!cvText.Contains('@') && !Regex.IsMatch(cvText, @"\b(inc\.|ltd|llc|gmbh|company|corp)\b", RegexOptions.IgnoreCase))
            weak.Add("No obvious employer or company names found — add a work history section with organization names.");

        return new AiCvAnalysisResultDto
        {
            ProfessionalSummary = string.IsNullOrWhiteSpace(preview) ? "CV text was extracted, but AI parsing was unavailable." : preview,
            Skills = DetectSkills(cvText),
            Notes = ["Fallback parsing was used because structured AI output was unavailable."],
            MissingOrWeakSections = weak,
            HowToImprove =
            [
                "Paste the full CV as plain text or upload a PDF/DOCX again if text extraction was poor.",
                "Add reverse-chronological jobs with title, company, dates, and bullet achievements.",
                "Include a skills section and at least one line on education or training.",
            ],
            HelpfulResources =
            [
                "Indeed — How to write a resume — https://www.indeed.com/career-advice/resumes-cover-letters/how-to-make-a-resume",
                "Coursera — Resume writing tips — https://www.coursera.org/articles/how-to-write-a-resume",
                "Novorésumé — Resume examples — https://novoresume.com/resume-examples",
                "LinkedIn — Profile and resume tips — https://www.linkedin.com/business/talent/blog/product-tips/how-to-write-a-resume-guide",
            ],
            SourceTextPreview = preview
        };
    }

    private static List<string> DetectSkills(string text)
    {
        var known = new[]
        {
            "C#", ".NET", "ASP.NET", "ASP.NET Core", "SQL", "PostgreSQL", "JavaScript", "TypeScript",
            "Blazor", "React", "Angular", "HTML", "CSS", "Docker", "Git", "Entity Framework", "REST API",
            "Azure", "AWS", "Python", "Java", "Node.js"
        };

        return known.Where(skill => text.Contains(skill, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static string BuildFallbackSkillGapSummary(string jobTitle, int score, List<string> matched, List<string> missing, int userExpYears, int requiredYears)
    {
        var summary = $"Your estimated fit for {jobTitle} is {score}/100.";
        if (matched.Count > 0) summary += $" Strong matches: {string.Join(", ", matched.Take(4))}.";
        if (missing.Count > 0) summary += $" Missing or weaker areas: {string.Join(", ", missing.Take(4))}.";
        summary += requiredYears > 0 ? $" Experience: {userExpYears} years vs {requiredYears} required." : $" Experience: {userExpYears} years.";
        return summary;
    }

    private static List<string> BuildNextSteps(List<string> missingSkillNames, int userExpYears, int requiredYears)
    {
        var steps = missingSkillNames.Take(3).Select(skill => $"Add evidence of {skill} in your profile or CV").ToList();
        if (requiredYears > userExpYears)
            steps.Add($"Highlight projects that show depth close to the {requiredYears}-year expectation");
        if (steps.Count == 0)
            steps.Add("Keep your profile updated with measurable achievements");
        return steps;
    }

    private static string BuildFallbackCoverLetter(string fullName, string jobTitle, string? aboutMe, List<string> skills, string? extraContext)
    {
        var intro = $"Dear Hiring Team,\n\nI am writing to apply for the {jobTitle} role. My name is {fullName}, and I believe my background aligns well with this opportunity.";
        var body = string.IsNullOrWhiteSpace(aboutMe)
            ? $" I bring experience with {JoinOrNone(skills)} and would be excited to contribute value from day one."
            : $" {aboutMe} I bring hands-on experience with {JoinOrNone(skills)}.";
        var close = string.IsNullOrWhiteSpace(extraContext)
            ? "\n\nThank you for your time and consideration. I would welcome the chance to discuss how I can contribute.\n\nSincerely,\n" + fullName
            : $"\n\n{extraContext}\n\nThank you for your time and consideration.\n\nSincerely,\n{fullName}";
        return intro + body + close;
    }

    private static string BuildFallbackMessage(string recipientName, string purpose, string? extraContext)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Hi {recipientName},");
        builder.AppendLine();
        builder.AppendLine($"I’m reaching out regarding {purpose}.");
        if (!string.IsNullOrWhiteSpace(extraContext))
        {
            builder.AppendLine();
            builder.AppendLine(extraContext.Trim());
        }
        builder.AppendLine();
        builder.AppendLine("I’d be glad to continue the conversation when convenient.");
        builder.AppendLine();
        builder.Append("Best regards");
        return builder.ToString();
    }

    private static List<string> NormalizeDistinct(IEnumerable<string>? items)
    {
        return items == null
            ? []
            : items.Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => NormalizeWhitespace(x.Trim()))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    private static string TrimLength(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var normalized = NormalizeWhitespace(value);
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string JoinOrNone(IEnumerable<string> values)
    {
        var list = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        return list.Count == 0 ? "none" : string.Join(", ", list);
    }
}
