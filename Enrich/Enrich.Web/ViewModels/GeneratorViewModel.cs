using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class GeneratorViewModel
    {
        public string Title { get; set; } = "Auto-generated collection";

        public string? Description { get; set; }

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public List<string> PartsOfSpeech { get; set; } = ["Any", "Noun", "Verb", "Adjective", "Adverb", "Preposition", "Conjunction", "Interjection", "Pronoun"];

        public List<string> DifficultyLevels { get; set; } = ["A1", "A2", "B1", "B2", "C1", "C2"];
    }
}
