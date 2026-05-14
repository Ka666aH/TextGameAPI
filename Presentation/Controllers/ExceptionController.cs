using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.DTO;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [BypassRefresh]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/exception")]
    public class ExceptionController : ControllerBase
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult Handle()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            return MapException(exception);
        }
        private IActionResult MapException(Exception? exception)
        {
            var originalPath = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? HttpContext.Request.Path;

            if (exception is ValidationException validationEx)
            {
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var problem = new ValidationProblemDetails(errors)
                {
                    Title = ExceptionsLabels.ValidationErrorCode,
                    Detail = ExceptionsLabels.ValidationErrorMessage,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = originalPath
                };

                return new ObjectResult(problem) { StatusCode = problem.Status };
            }

            if (exception is not GameException gameEx)
                return InternalServerError(originalPath, exception?.Message);

            return gameEx switch
            {
                IncorrectPasswordException =>
                    Problem(401, originalPath, gameEx),

                AccessTokenNotFoundException or
                MissingUserIdClaimException or
                MissingGameSessionIdClaimException or
                RefreshTokenNotFoundException or
                RefreshTokenExpiredException or
                RefreshTokenCompromisedException =>
                    DeleteCookieAndProblem(401, originalPath, gameEx),

                DefeatException e => Ok(new GameOverDTO(e.Message, e.GameInfo)),
                WinException e => Ok(new GameOverDTO(e.Message, e.GameInfo)),

                BattleWinException e => Ok(new BattleWinDTO(e.Message, e.BattleLog)),

                NullRoomIdException or
                NullItemIdException or
                EmptyException or
                NullEnemyIdException or
                UserNotFoundException or
                GameSessionNotFoundException =>
                    Problem(404, originalPath, gameEx),

                InvalidIdException or
                UncarryableException or
                ImpossibleStealException or
                UnsellableItemException =>
                    Problem(422, originalPath, gameEx),

                NotGameSessionOwnerException or
                UnstartedGameException or
                LockedException or
                NoKeyException or
                NoMapException or
                ClosedException or
                UndiscoveredRoomException or
                InBattleException or
                UnsearchedRoomException or
                NotShopException or
                NoMoneyException =>
                    Problem(403, originalPath, gameEx),

                _ => InternalServerError(originalPath, gameEx?.Message)
            };
        }

        private IActionResult Problem(int statusCode, string instance, GameException ex) =>
            Problem(statusCode: statusCode, title: ex.Code, detail: ex.Message, instance: instance);

        private IActionResult InternalServerError(string instance, string? detail = null) =>
            Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: ExceptionsLabels.InternalServerErrorCode,
                detail: detail ?? ExceptionsLabels.InternalServerErrorMessage,
                instance: HttpContext.Request.Path
            );
        private IActionResult DeleteCookieAndProblem(int statusCode, string instance, GameException ex)
        {
            HttpContext.Response.DeleteAuthCookies();
            return Problem(statusCode, instance, ex);
        }
    }
}