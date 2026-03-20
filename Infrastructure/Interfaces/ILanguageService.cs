using Domain.DTOs;

public interface ILanguageService
{
    Task<Response<string>> CreateAsync(CreateLanguageDto dto);
    Task<Response<Language>> GetByIdAsync(int id);
    Task<Response<List<Language>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateLanguageDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Language>>> SearchByNameAsync(string name);
}
