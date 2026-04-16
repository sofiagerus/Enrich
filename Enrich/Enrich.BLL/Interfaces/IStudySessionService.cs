using Enrich.BLL.Common;
using Enrich.BLL.DTOs;

namespace Enrich.BLL.Interfaces
{
    public interface IStudySessionService
    {
        Task<Result<StudySessionDTO>> StartStudySessionAsync(string userId, int bundleId);

        Task<Result<StudyProgressDTO>> SubmitAnswerAsync(string userId, SubmitAnswerDTO dto);

        Task<Result> FinishStudySessionAsync(string userId, int sessionId);

        Task<Result<StudyProgressDTO>> GetSessionProgressAsync(string userId, int sessionId);
    }
}
