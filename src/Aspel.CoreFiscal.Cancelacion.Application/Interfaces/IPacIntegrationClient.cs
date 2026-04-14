using Aspel.CoreFiscal.Cancelacion.Domain.Entities;

namespace Aspel.CoreFiscal.Cancelacion.Application.Interfaces
{
    public class PacIntegrationResult
    {
        public bool IsSuccess { get; init; }
        public string Acuse { get; init; } = string.Empty;
        public Domain.Enums.CfdiState NewState { get; init; }
        public bool IsHttpError { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
    }

    public interface IPacIntegrationClient
    {
        Task<PacIntegrationResult> CancelCfdiAsync(int pacId, CfdiDocument document, string xmlConfig, string xmlDocBase64, CancellationToken cancellationToken);
    }
}
