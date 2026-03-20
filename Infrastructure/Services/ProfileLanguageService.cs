using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class ProfileLanguageService(ApplicationDbContext dbContext) : IProfileLanguageService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<ProfileLanguage>> AddAsync(CreateProfileLanguageDto dto)
    {
        var profileLanguage = new ProfileLanguage
        {
            ProfileId = dto.ProfileId,
            LanguageId = dto.LanguageId,
            Level = dto.Level
        };
        await context.ProfileLanguages.AddAsync(profileLanguage);
        await context.SaveChangesAsync();
        return new Response<ProfileLanguage>(HttpStatusCode.OK, "Add ProfileLanguage successfully", profileLanguage);
    }

    public async Task<Response<ProfileLanguage>> GetByIdAsync(int id)
    {
        var get = await context.ProfileLanguages.FindAsync(id);
        if (get == null)
            return new Response<ProfileLanguage>(HttpStatusCode.NotFound, "ProfileLanguage not found");
        return new Response<ProfileLanguage>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<ProfileLanguage>>> GetAllAsync()
    {
        var list = await context.ProfileLanguages.ToListAsync();
        return new Response<List<ProfileLanguage>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<ProfileLanguage>> UpdateAsync(int id, UpdateProfileLanguageDto dto)
    {
        var update = await context.ProfileLanguages.FindAsync(id);
        if (update == null)
            return new Response<ProfileLanguage>(HttpStatusCode.NotFound, "ProfileLanguage not found");

        update.LanguageId = dto.LanguageId;
        update.Level = dto.Level;
        await context.SaveChangesAsync();
        return new Response<ProfileLanguage>(HttpStatusCode.OK, "ok", update);
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.ProfileLanguages.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "ProfileLanguage not found");

        context.ProfileLanguages.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted ProfileLanguage successfully");
    }

    public async Task<Response<List<ProfileLanguage>>> GetByProfileIdAsync(int profileId)
    {
        var list = await context.ProfileLanguages.Where(x => x.ProfileId == profileId).ToListAsync();
        return new Response<List<ProfileLanguage>>(HttpStatusCode.OK, "ok", list);
    }
}
