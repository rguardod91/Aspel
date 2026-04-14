using Aspel.CoreFiscal.Cancelacion.Domain.Entities;

namespace Aspel.CoreFiscal.Cancelacion.Application.Interfaces
{
    public interface ICfdiCancellationRepository
    {
        Task<CfdiDocument?> GetByUuidAsync(string uuid, CancellationToken cancellationToken);
        Task SaveAsync(CfdiDocument document, CancellationToken cancellationToken);
        Task UpdateStatusAsync(CfdiDocument document, CancellationToken cancellationToken);
    }
}
