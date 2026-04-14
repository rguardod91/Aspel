using Aspel.CoreFiscal.Cancelacion.Application.UseCases.Cancelacion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Aspel.CoreFiscal.Cancelacion.Api.Endpoints
{
    public static class CancelacionEndpoints
    {
        public static void MapCancelacionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/cancelaciones")
                           .WithTags("Cancelaciones")
                           .WithOpenApi();

            group.MapPost("/", async ([FromBody] CancelCfdiCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .Produces<Application.DTOs.CancellationResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ProcesarCancelacion")
            .WithSummary("Inicia el proceso de cancelación de un CFDI a través del balanceador de PACs.");
        }
    }
}
