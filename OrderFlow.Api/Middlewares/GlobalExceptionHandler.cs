using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderFlow.Api.Middlewares
{
    internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Exceção capturada: {Message}", exception.Message);

            var problemDetails = new ProblemDetails();

            if (exception is ValidationException validationException)
            {
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Erro de validação";
                problemDetails.Detail = "Um ou mais erros de validação ocorreram.";

                // Agrupa os erros pelo nome do campo
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(e => e.ErrorMessage).ToArray());
            }
            else
            {
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Erro Interno no Servidor";
                problemDetails.Detail = "Ocorreu um erro inesperado ao processar sua solicitação.";
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Avisa ao ASP.NET Core que a exceção foi tratada com sucesso
        }
    }
}
