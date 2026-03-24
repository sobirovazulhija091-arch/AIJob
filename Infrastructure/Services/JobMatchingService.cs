using System.Net;
using Domain.DTOs;
using Domain.Entities;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class JobMatchingService(ApplicationDbContext context, IGoogleAiService aiService) : IJobMatchingService
{
    public async Task<PagedResult<JobWithMatchDto>> GetRecommendedJobsForUserAsync(int userId, JobFilter? filter, PagedQuery query)
    {
        var userSkillIds = await GetUserSkillIdsAsync(userId);
        var userExpYears = await GetUserExperienceYearsAsync(userId);

        var jobsQuery = context.Jobs.AsQueryable();
        if (filter != null)
        {
            if (filter.OrganizationId.HasValue)
                jobsQuery = jobsQuery.Where(j => j.OrganizationId == filter.OrganizationId.Value);
            if (filter.CategoryId.HasValue)
                jobsQuery = jobsQuery.Where(j => j.CategoryId == filter.CategoryId.Value);
            if (!string.IsNullOrEmpty(filter.Title))
                jobsQuery = jobsQuery.Where(j => j.Title.Contains(filter.Title));
            if (!string.IsNullOrEmpty(filter.Location))
                jobsQuery = jobsQuery.Where(j => j.Location != null && j.Location.Contains(filter.Location));
            if (filter.JobType.HasValue)
                jobsQuery = jobsQuery.Where(j => j.JobType == filter.JobType.Value);
            if (filter.ExperienceLevel.HasValue)
                jobsQuery = jobsQuery.Where(j => j.ExperienceLevel == filter.ExperienceLevel.Value);
            if (filter.SalaryMin.HasValue)
                jobsQuery = jobsQuery.Where(j => j.SalaryMin >= filter.SalaryMin.Value);
            if (filter.SalaryMax.HasValue)
                jobsQuery = jobsQuery.Where(j => j.SalaryMax <= filter.SalaryMax.Value);
        }

        var jobs = await jobsQuery.ToListAsync();
        var jobSkillDict = await GetJobSkillIdsAsync(jobs.Select(j => j.Id).ToList());

        var scored = new List<JobWithMatchDto>();
        foreach (var job in jobs)
        {
            var jobSkillIds = jobSkillDict.GetValueOrDefault(job.Id, new List<int>());
            var (score, summary) = ComputeJobMatch(userSkillIds, userExpYears, job, jobSkillIds);
            scored.Add(new JobWithMatchDto { Job = job, MatchScore = score, MatchSummary = summary });
        }

        scored = scored.OrderByDescending(x => x.MatchScore).ToList();

        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;
        var total = scored.Count;
        var items = scored.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<JobWithMatchDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<PagedResult<ApplicantWithMatchDto>> GetRecommendedApplicantsForJobAsync(int jobId, PagedQuery query)
    {
        var job = await context.Jobs.FindAsync(jobId);
        if (job == null)
            return new PagedResult<ApplicantWithMatchDto> { Page = 1, PageSize = query.PageSize, TotalCount = 0, TotalPages = 0 };

        var applications = await context.JobApplications
            .Where(ja => ja.JobId == jobId)
            .ToListAsync();

        var jobSkillIds = await context.JobSkills.Where(js => js.JobId == jobId).Select(js => js.SkillId).ToListAsync();
        var userIds = applications.Select(a => a.UserId).Distinct().ToList();
        var users = await context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);
        var userSkillsDict = await GetUserSkillIdsByUserIdsAsync(userIds);
        var userExpDict = await GetUserExperienceYearsByUserIdsAsync(userIds);
        var userProfileDict = await GetUserProfileByUserIdsAsync(userIds);

        var scored = new List<ApplicantWithMatchDto>();
        foreach (var app in applications)
        {
            var userSkillIds = userSkillsDict.GetValueOrDefault(app.UserId, new List<int>());
            var userExpYears = userExpDict.GetValueOrDefault(app.UserId, 0);
            var profile = userProfileDict.GetValueOrDefault(app.UserId);
            var (score, summary) = ComputeJobMatch(userSkillIds, userExpYears, job, jobSkillIds);

            var user = users.GetValueOrDefault(app.UserId);
            if (user == null) continue;
            scored.Add(new ApplicantWithMatchDto
            {
                Application = app,
                User = user,
                UserProfileAbout = profile?.AboutMe,
                ExperienceYears = userExpYears,
                MatchScore = score,
                MatchSummary = summary
            });
        }

        scored = scored.OrderByDescending(x => x.MatchScore).ToList();

        var page = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;
        var total = scored.Count;
        var items = scored.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<ApplicantWithMatchDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    private async Task<List<int>> GetUserSkillIdsAsync(int userId)
    {
        var fromUserSkill = await context.UserSkills.Where(us => us.UserId == userId).Select(us => us.SkillId).ToListAsync();
        var profile = await context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return fromUserSkill;
        var fromProfileSkill = await context.ProfileSkills.Where(ps => ps.ProfileId == profile.Id).Select(ps => ps.SkillId).ToListAsync();
        return fromUserSkill.Union(fromProfileSkill).Distinct().ToList();
    }

    private async Task<int> GetUserExperienceYearsAsync(int userId)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null && profile.ExperienceYears > 0) return profile.ExperienceYears;
        var experiences = await context.UserExperiences.Where(e => e.UserId == userId).ToListAsync();
        var totalMonths = experiences.Sum(e => (int)((e.EndDate - e.StartDate).TotalDays / 30));
        return totalMonths / 12;
    }

    private async Task<Dictionary<int, List<int>>> GetJobSkillIdsAsync(List<int> jobIds)
    {
        if (jobIds.Count == 0) return new Dictionary<int, List<int>>();
        var pairs = await context.JobSkills.Where(js => jobIds.Contains(js.JobId)).ToListAsync();
        return pairs.GroupBy(p => p.JobId).ToDictionary(g => g.Key, g => g.Select(x => x.SkillId).Distinct().ToList());
    }

    private async Task<Dictionary<int, List<int>>> GetUserSkillIdsByUserIdsAsync(List<int> userIds)
    {
        if (userIds.Count == 0) return new Dictionary<int, List<int>>();
        var userSkillPairs = await context.UserSkills.Where(us => userIds.Contains(us.UserId)).ToListAsync();
        var profiles = await context.Profiles.Where(p => userIds.Contains(p.UserId)).ToListAsync();
        var profileIds = profiles.Select(p => p.Id).ToList();
        var profileSkillPairs = profileIds.Count > 0
            ? await context.ProfileSkills.Where(ps => profileIds.Contains(ps.ProfileId)).ToListAsync()
            : new List<ProfileSkill>();
        var profileByUserId = profiles.ToDictionary(p => p.UserId);

        var result = new Dictionary<int, List<int>>();
        foreach (var uid in userIds)
        {
            var skillIds = userSkillPairs.Where(us => us.UserId == uid).Select(us => us.SkillId).ToList();
            if (profileByUserId.TryGetValue(uid, out var prof))
                skillIds = skillIds.Union(profileSkillPairs.Where(ps => ps.ProfileId == prof.Id).Select(ps => ps.SkillId)).Distinct().ToList();
            result[uid] = skillIds;
        }
        return result;
    }

    private async Task<Dictionary<int, int>> GetUserExperienceYearsByUserIdsAsync(List<int> userIds)
    {
        if (userIds.Count == 0) return new Dictionary<int, int>();
        var profiles = await context.UserProfiles.Where(p => userIds.Contains(p.UserId)).ToListAsync();
        var experiences = await context.UserExperiences.Where(e => userIds.Contains(e.UserId)).ToListAsync();

        var result = new Dictionary<int, int>();
        foreach (var uid in userIds)
        {
            var profile = profiles.FirstOrDefault(p => p.UserId == uid);
            if (profile != null && profile.ExperienceYears > 0)
            {
                result[uid] = profile.ExperienceYears;
                continue;
            }
            var userExps = experiences.Where(e => e.UserId == uid).ToList();
            var totalMonths = userExps.Sum(e => (int)((e.EndDate - e.StartDate).TotalDays / 30));
            result[uid] = totalMonths / 12;
        }
        return result;
    }

    private async Task<Dictionary<int, UserProfile>> GetUserProfileByUserIdsAsync(List<int> userIds)
    {
        if (userIds.Count == 0) return new Dictionary<int, UserProfile>();
        var profiles = await context.UserProfiles.Where(p => userIds.Contains(p.UserId)).ToListAsync();
        return profiles.ToDictionary(p => p.UserId);
    }

    private static (int Score, string Summary) ComputeJobMatch(List<int> userSkillIds, int userExpYears, Job job, List<int> jobSkillIds)
    {
        var skillScore = 0;
        var skillSummary = "";
        if (jobSkillIds.Count > 0)
        {
            var matched = userSkillIds.Intersect(jobSkillIds).Count();
            skillScore = (int)Math.Round((double)matched / jobSkillIds.Count * 60); // up to 60 points
            skillSummary = matched > 0 ? $"{matched}/{jobSkillIds.Count} skills match" : "No required skills yet";
        }
        else
        {
            skillScore = 30; // no job skills defined, give neutral
            skillSummary = "No required skills defined";
        }

        var expScore = 0;
        var expSummary = "";
        var required = job.ExperienceRequired;
        if (userExpYears >= required)
        {
            expScore = 40;
            expSummary = userExpYears > required ? $"{userExpYears}y exp (exceeds {required}y)" : $"{userExpYears}y exp matches";
        }
        else if (required > 0)
        {
            var ratio = (double)userExpYears / required;
            expScore = (int)Math.Round(ratio * 30); // up to 30 if under
            expSummary = $"{userExpYears}y vs {required}y required";
        }
        else
        {
            expScore = 20;
            expSummary = "Experience optional";
        }

        var total = Math.Min(100, skillScore + expScore);
        var summary = $"{skillSummary}. {expSummary}.";
        return (total, summary);
    }

    public async Task<Response<string>> GetMatchExplanationAsync(int userId, int jobId, bool useAi = false)
    {
        var job = await context.Jobs.FindAsync(jobId);
        if (job == null)
            return new Response<string>(HttpStatusCode.NotFound, "Job not found");

        var userSkillIds = await GetUserSkillIdsAsync(userId);
        var userExpYears = await GetUserExperienceYearsAsync(userId);
        var jobSkillIds = await context.JobSkills.Where(js => js.JobId == jobId).Select(js => js.SkillId).ToListAsync();
        var jobSkillNames = jobSkillIds.Count > 0
            ? await context.Skills.Where(s => jobSkillIds.Contains(s.Id)).Select(s => s.Name).ToListAsync()
            : new List<string>();
        var userSkillNames = userSkillIds.Count > 0
            ? await context.Skills.Where(s => userSkillIds.Contains(s.Id)).Select(s => s.Name).ToListAsync()
            : new List<string>();

        var (score, summary) = ComputeJobMatch(userSkillIds, userExpYears, job, jobSkillIds);

        if (useAi)
        {
            var prompt = $@"Analyze job-candidate fit. Job: ""{job.Title}"". Required skills: {string.Join(", ", jobSkillNames)}. Required experience: {job.ExperienceRequired} years. Candidate skills: {string.Join(", ", userSkillNames)}. Candidate experience: {userExpYears} years. Match score: {score}/100. Give a brief 1-2 sentence professional explanation of the fit.";
            var aiResult = await aiService.AskAsync(new CreateAiPromptDto { Prompt = prompt });
            if (aiResult.StatusCode == (int)HttpStatusCode.OK && !string.IsNullOrWhiteSpace(aiResult.Data))
                return new Response<string>(HttpStatusCode.OK, "ok", aiResult.Data!);
        }

        return new Response<string>(HttpStatusCode.OK, "ok", summary);
    }
}
