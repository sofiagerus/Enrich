using System.Text.Json.Serialization;

namespace Enrich.BLL.DTOs.DictionaryApi
{
    public class FreeDictionaryEntryDto
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("phonetic")]
        public string? Phonetic { get; set; }

        [JsonPropertyName("phonetics")]
        public List<PhoneticDto>? Phonetics { get; set; }

        [JsonPropertyName("meanings")]
        public List<MeaningDto>? Meanings { get; set; }
    }

    public class PhoneticDto
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("audio")]
        public string? Audio { get; set; }
    }

    public class MeaningDto
    {
        [JsonPropertyName("partOfSpeech")]
        public string? PartOfSpeech { get; set; }

        [JsonPropertyName("definitions")]
        public List<DefinitionDto>? Definitions { get; set; }
    }

    public class DefinitionDto
    {
        [JsonPropertyName("definition")]
        public string? Definition { get; set; }

        [JsonPropertyName("example")]
        public string? Example { get; set; }
    }
}
