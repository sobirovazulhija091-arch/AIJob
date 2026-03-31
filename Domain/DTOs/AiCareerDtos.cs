using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class AiCvAnalysisRequestDto
{
    public int? UserId { get; set; }
    public string? CvText { get; set; }
    public string? CvFileUrl { get; set; }
    public bool ApplyToProfile { get; set; }
    public bool SyncSkills { get; set; }
}

public class AiCvAnalysisResultDto
{
    public string FullName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string ProfessionalSummary { get; set; } = "";
    public int ExperienceYears { get; set; }
    public List<string> Skills { get; set; } = [];
    public List<string> Education { get; set; } = [];
    public List<string> RecommendedRoles { get; set; } = [];
    public List<string> Notes { get; set; } = [];
    /// <summary>Concrete gaps: missing dates, no jobs listed, no skills section, etc.</summary>
    public List<string> MissingOrWeakSections { get; set; } = [];
    /// <summary>Actionable steps to improve the CV.</summary>
    public List<string> HowToImprove { get; set; } = [];
    /// <summary>Reputable resources, e.g. "Title — https://..."</summary>
    public List<string> HelpfulResources { get; set; } = [];
    public string SourceTextPreview { get; set; } = "";
}

public class AiSkillGapResultDto
{
    public int MatchScore { get; set; }
    public string FitSummary { get; set; } = "";
    public List<string> Strengths { get; set; } = [];
    public List<string> MissingSkills { get; set; } = [];
    public List<string> NextSteps { get; set; } = [];
}

public class AiJobImproveRequestDto
{
    public int? JobId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int ExperienceRequired { get; set; }
    public bool ApplyToJob { get; set; }
}

public class AiJobImproveResultDto
{
    public string ImprovedTitle { get; set; } = "";
    public string ImprovedDescription { get; set; } = "";
    public List<string> SuggestedSkills { get; set; } = [];
    public List<string> SuggestedResponsibilities { get; set; } = [];
    public List<string> SuggestedBenefits { get; set; } = [];
}

public class AiDraftCoverLetterRequestDto
{
    public int UserId { get; set; }
    public int JobId { get; set; }
    public string? Tone { get; set; }
    public string? ExtraContext { get; set; }
}

public class AiDraftMessageRequestDto
{
    public int UserId { get; set; }
    public int? JobId { get; set; }
    public string RecipientName { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string? Tone { get; set; }
    public string? ExtraContext { get; set; }
}

public class AiDraftResultDto
{
    public string Subject { get; set; } = "";
    public string Content { get; set; } = "";
}

public class AiCvAnalysisJsonDto
{
    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("professionalSummary")]
    public string? ProfessionalSummary { get; set; }

    [JsonPropertyName("experienceYears")]
    public int? ExperienceYears { get; set; }

    [JsonPropertyName("skills")]
    public List<string>? Skills { get; set; }

    [JsonPropertyName("education")]
    public List<string>? Education { get; set; }

    [JsonPropertyName("recommendedRoles")]
    public List<string>? RecommendedRoles { get; set; }

    [JsonPropertyName("notes")]
    public List<string>? Notes { get; set; }

    [JsonPropertyName("missingOrWeakSections")]
    public List<string>? MissingOrWeakSections { get; set; }

    [JsonPropertyName("howToImprove")]
    public List<string>? HowToImprove { get; set; }

    [JsonPropertyName("helpfulResources")]
    public List<string>? HelpfulResources { get; set; }
}

public class AiSkillGapJsonDto
{
    [JsonPropertyName("fitSummary")]
    public string? FitSummary { get; set; }

    [JsonPropertyName("strengths")]
    public List<string>? Strengths { get; set; }

    [JsonPropertyName("missingSkills")]
    public List<string>? MissingSkills { get; set; }

    [JsonPropertyName("nextSteps")]
    public List<string>? NextSteps { get; set; }
}

public class AiJobImproveJsonDto
{
    [JsonPropertyName("improvedTitle")]
    public string? ImprovedTitle { get; set; }

    [JsonPropertyName("improvedDescription")]
    public string? ImprovedDescription { get; set; }

    [JsonPropertyName("suggestedSkills")]
    public List<string>? SuggestedSkills { get; set; }

    [JsonPropertyName("suggestedResponsibilities")]
    public List<string>? SuggestedResponsibilities { get; set; }

    [JsonPropertyName("suggestedBenefits")]
    public List<string>? SuggestedBenefits { get; set; }
}

public class AiDraftJsonDto
{
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
