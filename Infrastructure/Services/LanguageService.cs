using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class LanguageService(ApplicationDbContext dbContext) : ILanguageService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(CreateLanguageDto dto)
    {
        var language = new Language { Name = dto.Name, Type = dto.Type };
        await context.Languages.AddAsync(language);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Add Language successfully");
    }

    public async Task<Response<Language>> GetByIdAsync(int id)
    {
        var get = await context.Languages.FindAsync(id);
        if (get == null)
            return new Response<Language>(HttpStatusCode.NotFound, "Language not found");
        return new Response<Language>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Language>>> GetAllAsync()
    {
        var list = await context.Languages.ToListAsync();
        return new Response<List<Language>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, UpdateLanguageDto dto)
    {
        var update = await context.Languages.FindAsync(id);
        if (update == null)
            return new Response<string>(HttpStatusCode.NotFound, "Language not found");

        update.Name = dto.Name;
        update.Type = dto.Type;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id)
    {
        var del = await context.Languages.FindAsync(id);
        if (del == null)
            return new Response<string>(HttpStatusCode.NotFound, "Language not found");

        context.Languages.Remove(del);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Deleted Language successfully");
    }

    public async Task<Response<List<Language>>> SearchByNameAsync(string name)
    {
        var list = await context.Languages.Where(x => x.Name.Contains(name)).ToListAsync();
        return new Response<List<Language>>(HttpStatusCode.OK, "ok", list);
    }
}
