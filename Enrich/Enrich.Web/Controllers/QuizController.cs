using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class QuizController(
        IQuizService quizService,
        IStudySessionService studySessionService,
        ILogger<QuizController> logger) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            logger.LogInformation("User {UserId} accessed quiz setup.", CurrentUserId);
            var categoriesResult = await quizService.GetAvailableCategoriesAsync(CurrentUserId);
            var partsOfSpeechResult = await quizService.GetAvailablePartsOfSpeechAsync(CurrentUserId);

            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : new List<string>();
            ViewBag.PartsOfSpeech = partsOfSpeechResult.IsSuccess ? partsOfSpeechResult.Value : new List<string>();

            return View(new QuizSetupDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Start(QuizSetupDTO setup)
        {
            logger.LogInformation("User {UserId} is starting a custom quiz session.", CurrentUserId);
            var result = await quizService.StartCustomQuizAsync(CurrentUserId, setup);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Setup));
            }

            return View("Session", result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDTO dto)
        {
            var result = await studySessionService.SubmitAnswerAsync(CurrentUserId, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Json(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int sessionId)
        {
            logger.LogInformation("User {UserId} is finishing session {SessionId}.", CurrentUserId, sessionId);
            var result = await studySessionService.FinishStudySessionAsync(CurrentUserId, sessionId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return RedirectToAction(nameof(Result), new { sessionId });
        }

        [HttpGet]
        public async Task<IActionResult> Result(int sessionId)
        {
            logger.LogInformation("User {UserId} is viewing results for session {SessionId}.", CurrentUserId, sessionId);
            var progressResult = await studySessionService.GetSessionProgressAsync(CurrentUserId, sessionId);

            if (!progressResult.IsSuccess)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(progressResult.Value);
        }
    }
}
