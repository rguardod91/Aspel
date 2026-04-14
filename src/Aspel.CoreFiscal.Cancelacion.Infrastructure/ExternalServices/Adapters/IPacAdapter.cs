using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices.Adapters
{
    public interface IPacAdapter
    {
        int PacId { get; }
        Task<PacIntegrationResult> ExecuteCancellationAsync(CfdiDocument document, string xmlConfig50, string xmlDocBase64, CancellationToken cancellationToken);
    }
}
