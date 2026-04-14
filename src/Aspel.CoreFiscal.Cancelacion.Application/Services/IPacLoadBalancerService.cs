using Aspel.CoreFiscal.Cancelacion.Domain.Entities;

namespace Aspel.CoreFiscal.Cancelacion.Application.Services
{
    public interface IPacLoadBalancerService
    {
        Task<PacNode> GetBestAvailablePacAsync(int defaultPacId, CancellationToken cancellationToken);
        Task ReportPacPerformanceAsync(int pacId, long durationMs, bool isHttpError, CancellationToken cancellationToken);
    }
}
