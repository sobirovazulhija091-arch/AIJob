using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class ProfileService(ApplicationDbContext dbContext) : IProfileService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateProfileDto dto)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
            return new Response<string>(HttpStatusCode.BadRequest, "User does not exist");

        var existingProfile = await context.Profiles.FirstOrDefaultAsync(x => x.UserId == dto.UserId);
        if (existingProfile != null)
            return new Response<string>(HttpStatusCode.BadRequest, "User already has a profile. One profile per user.");

        var profile = new Profile
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Headline = dto.Headline,
            About = dto.About,
            Location = dto.Location,
            PhotoUrl = dto.PhotoUrl,
            BackgroundPhotoUrl = dto.BackgroundPhotoUrl,
            BirthDate = dto.BirthDate,
            CreatedAt = DateTime.UtcNow
        };
        await context.Profiles.AddAsync(profile);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Profile created successfully");
    }

    public async Task<Response<Profile>> GetByIdAsync(int id)
    {
        var get = await context.Profiles.FindAsync(id);
        if (get == null)
            return new Response<Profile>(HttpStatusCode.NotFound, "Profile not found");
        return new Response<Profile>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Profile>>> GetAllAsync()
    {
        var list = await context.Profiles.ToListAsync();
        return new Response<List<Profile>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateProfileDto dto)
    {
        var update = await context.Profiles.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "Profile not found");

        update.FirstName = dto.FirstName;
        update.LastName = dto.LastName;
        update.Headline = dto.Headline;
        update.About = dto.About;
        update.Location = dto.Location;
        update.PhotoUrl = dto.PhotoUrl;
        update.BackgroundPhotoUrl = dto.BackgroundPhotoUrl;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.Profiles.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "Profile not found");

        context.Profiles.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Profile deleted successfully");
    }

    public async Task<Response<Profile>> GetByUserIdAsync(int userId)
    {
        var get = await context.Profiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (get == null)
            return new Response<Profile>(HttpStatusCode.NotFound, "Profile not found");
        return new Response<Profile>(HttpStatusCode.OK, "ok", get);
    }
}
