using Enrich.BLL.DTOs;

namespace Enrich.BLL.Interfaces
{
    public interface IWordService
    {
        Task<(bool Success, string? ErrorMessage)> CreatePersonalWordAsync(string userId, CreatePersonalWordDTO dto);

        Task<IEnumerable<PersonalWordDTO>> GetPersonalWordsAsync(string userId);
    }
}
