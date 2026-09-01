using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.SubscriptionHistory.Middleware
{
    public class ValidationExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ValidationProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Ошибка валидации",
                    Detail = "Один или несколько параметров не прошли валидацию"
                };

                foreach (var error in ex.Errors)
                {
                    problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
                }

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}