using MarketData.Api.Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MarketData.Api.Common.Validation;

/// <summary>
/// Converts invalid model state into a consistent ProblemDetails response.
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var validationProblem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation error.",
            Detail = "One or more validation errors occurred."
        };

        validationProblem.Extensions["code"] = ErrorCodes.ValidationError.ToString();

        context.Result = new BadRequestObjectResult(validationProblem);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op
    }
}
