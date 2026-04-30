using Enrich.BLL.DTOs.DictionaryApi;

namespace Enrich.BLL.Interfaces
{
    public interface IDictionaryApiClient
    {
        Task<FreeDictionaryEntryDto?> GetWordDetailsAsync(string word);
    }
}
