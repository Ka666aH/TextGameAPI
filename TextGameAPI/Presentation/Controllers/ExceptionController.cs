using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.DTO;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.DTO;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [BypassRefresh]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/exception")]
    public class ExceptionController : ControllerBase
    {
        private readonly ILogger<ExceptionController> _logger;

        public ExceptionController(ILogger<ExceptionController> logger)
        {
            _logger = logger;
        }

        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult Handle()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            var result = MapException(exception);
            Log(exception, result);
            return result;
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
                return InternalServerError();

            return gameEx switch
            {
                IncorrectPasswordException =>
                    Problem(401, originalPath, gameEx),

                AccessTokenNotFoundException or
                MissingUserIdClaimException or
                MissingSessionIdClaimException or
                RefreshTokenNotFoundException or
                RefreshTokenExpiredException or
                RefreshTokenCompromisedException =>
                    DeleteCookieAndProblem(401, originalPath, gameEx),

                DefeatException e => Ok(new GameOverDTO(e.Message, e.GameInfo)),
                WinException e => Ok(new GameOverDTO(e.Message, e.GameInfo)),

                BattleWinException e => Ok(new BattleWinDTO(e.Message, e.BattleLog)),

                CorruptStateException =>
                    Problem(StatusCodes.Status500InternalServerError, originalPath, gameEx),

                RoomNotFoundException or
                ItemNotFoundException or
                NothingFoundException or
                EnemyNotFoundException or
                UserNotFoundException or
                SaveNotFoundException or
                SessionNotFoundException =>
                    Problem(404, originalPath, gameEx),

                InvalidIdException or
                UncarryableException or
                ImpossibleStealException or
                UnsellableItemException or //=> Problem(422, originalPath, gameEx),
                
                ImpossibleDeleteSaveException or
                NotSessionOwnerException or
                NotSaveOwnerException or
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

                _ => InternalServerError()
            };
        }

        private void Log(Exception? exception, IActionResult result)
        {
            var statusCode = result switch
            {
                ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
                StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
                _ => StatusCodes.Status200OK
            };

            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;
            var code = ResolveCode(exception);
            var traceId = HttpContext.TraceIdentifier;

            // 5xx — дефект, разбирать по стеку. 401/403 — ожидаемое поведение игры и авторизации,
            // стек там только шумит. Остальное (400, 404, 200-сценарии) — нормальный ход игры.
            if (statusCode >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, LoggersText.RequestFailed, method, path, statusCode, code, traceId);
            else if (statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
                _logger.LogWarning(LoggersText.RequestRejected, method, path, statusCode, code, traceId);
            else
                _logger.LogDebug(LoggersText.GameFlowCompleted, method, path, statusCode, code, traceId);
        }

        private static string ResolveCode(Exception? exception) => exception switch
        {
            ValidationException => ExceptionsLabels.ValidationErrorCode,
            GameException gameException => gameException.Code,
            _ => ExceptionsLabels.InternalServerErrorCode
        };

        private IActionResult Problem(int statusCode, string instance, GameException ex)
        {
            TryRefreshAuthCookies(HttpContext);
            return Problem(statusCode: statusCode, title: ex.Code, detail: ex.Message, instance: instance);
        }

        private IActionResult InternalServerError()
        {
            TryRefreshAuthCookies(HttpContext);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: ExceptionsLabels.InternalServerErrorCode,
                detail: ExceptionsLabels.InternalServerErrorMessage,
                instance: HttpContext.Request.Path
            );
        }
        private IActionResult DeleteCookieAndProblem(int statusCode, string instance, GameException ex)
        {
            HttpContext.Response.DeleteAuthCookies();
            return Problem(statusCode, instance, ex);
        }
        private static void TryRefreshAuthCookies(HttpContext context)
        {
            if (context.Items.TryGetValue(ItemKeys.RefreshResultKey, out var value) && value is AuthResult refreshResult)
            {
                CookieHelper.SetAuthCookies(refreshResult, context);
            }
        }
    }
}