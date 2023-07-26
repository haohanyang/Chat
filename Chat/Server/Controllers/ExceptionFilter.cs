using Microsoft.AspNetCore.Mvc.Filters;
using Chat.Server.Exceptions;
using Microsoft.AspNetCore.Mvc;


namespace Chat.Server.Controllers;
public class ExceptionFilter : IActionFilter
{

    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var exception = context.Exception;
        if (exception != null)
        {
            _logger.LogError(exception.Message);
            if (exception is UserNotFoundException)
            {
                context.Result = new NotFoundObjectResult(exception.Message);
                context.ExceptionHandled = true;
            }
            if (exception is AuthException)
            {
                context.Result = new UnauthorizedObjectResult(exception.Message);
                context.ExceptionHandled = true;
            }
            if (exception is BadCredentialException)
            {
                context.Result = new UnauthorizedObjectResult(exception.Message);
                context.ExceptionHandled = true;
            }
            else if (exception is ArgumentException)
            {
                context.Result = new BadRequestObjectResult(exception.Message);
                context.ExceptionHandled = true;
            }
            else if (exception is EnvironmentVariableException)
            {
                _logger.LogError(exception.Message);
                context.Result = new ObjectResult("Internal Server Error")
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
            }
            else
            {
                _logger.LogError(exception.Message);
                context.Result = new ObjectResult("Internal Server Error")
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
            }

        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}