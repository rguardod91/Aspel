using Aspel.CoreFiscal.Cancelacion.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aspel.CoreFiscal.Cancelacion.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            if (exception is CoreFiscalDomainException domainException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Error de Regla de Negocio";
                problemDetails.Detail = domainException.Message;
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Error Interno del Servidor";
                problemDetails.Detail = "Ha ocurrido un error inesperado al procesar la solicitud.";
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
