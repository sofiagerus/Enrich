using System.Net.Http.Json;
using Enrich.BLL.DTOs.DictionaryApi;
using Enrich.BLL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Clients
{
    public class DictionaryApiClient : IDictionaryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryApiClient> _logger;

        public DictionaryApiClient(HttpClient httpClient, ILogger<DictionaryApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FreeDictionaryEntryDto?> GetWordDetailsAsync(string word)
        {
            try
            {
                // Free Dictionary API format: https://api.dictionaryapi.dev/api/v2/entries/en/<word>
                var response = await _httpClient.GetAsync(word);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Free Dictionary API returned status {StatusCode} for word '{Word}'", response.StatusCode, word);
                    return null;
                }

                var entries = await response.Content.ReadFromJsonAsync<List<FreeDictionaryEntryDto>>();

                // Return the first entry if available
                return entries?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching definition for word '{Word}' from Free Dictionary API", word);
                return null;
            }
        }
    }
}
