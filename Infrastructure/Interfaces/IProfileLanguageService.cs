using Domain.DTOs;

public interface IProfileLanguageService
{
    Task<Response<ProfileLanguage>> AddAsync(CreateProfileLanguageDto dto);
    Task<Response<ProfileLanguage>> GetByIdAsync(int id);
    Task<Response<List<ProfileLanguage>>> GetAllAsync();
    Task<Response<ProfileLanguage>> UpdateAsync(int id, UpdateProfileLanguageDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<ProfileLanguage>>> GetByProfileIdAsync(int profileId);
}