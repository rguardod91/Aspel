using Aspel.CoreFiscal.Cancelacion.Domain.Entities;

namespace Aspel.CoreFiscal.Cancelacion.Application.Interfaces
{
    public interface IPacStateRepository
    {
        Task<IReadOnlyList<PacNode>> GetAllPacsAsync(CancellationToken cancellationToken);
        Task<PacNode?> GetPacByIdAsync(int pacId, CancellationToken cancellationToken);
        Task UpdatePacAsync(PacNode pacNode, CancellationToken cancellationToken);
    }
}
