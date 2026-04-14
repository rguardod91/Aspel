using Aspel.CoreFiscal.Cancelacion.Application.DTOs;
using MediatR;

namespace Aspel.CoreFiscal.Cancelacion.Application.UseCases.Cancelacion
{
    public record CancelCfdiCommand(
        string Uuid,
        string RfcEmisor,
        string RfcReceptor,
        decimal Total,
        string XmlDocBase64,
        string XmlConfig50,
        int PacDefaultId = 7 // Valor por defecto heredado de configuraciones legacy
    ) : IRequest<CancellationResultDto>;
}
