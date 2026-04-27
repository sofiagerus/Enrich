using Enrich.BLL.Common;
using Enrich.BLL.DTOs;

namespace Enrich.BLL.Interfaces
{
    public interface IQuizService
    {
        Task<Result<StudySessionDTO>> StartCustomQuizAsync(string userId, QuizSetupDTO setup);

        Task<Result<IEnumerable<string>>> GetAvailableCategoriesAsync(string userId);

        Task<Result<IEnumerable<string>>> GetAvailablePartsOfSpeechAsync(string userId);
    }
}
