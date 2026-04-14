using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices.Adapters;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices
{
    public class PacIntegrationClient : IPacIntegrationClient
    {
        private readonly IEnumerable<IPacAdapter> _adapters;

        public PacIntegrationClient(IEnumerable<IPacAdapter> adapters)
        {
            _adapters = adapters;
        }

        public async Task<PacIntegrationResult> CancelCfdiAsync(int pacId, CfdiDocument document, string xmlConfig50, string xmlDocBase64, CancellationToken cancellationToken)
        {
            var adapter = _adapters.FirstOrDefault(a => a.PacId == pacId);

            if (adapter == null)
            {
                throw new InvalidOperationException($"No se encontró un adaptador configurado para el PAC con ID {pacId}.");
            }

            try
            {
                return await adapter.ExecuteCancellationAsync(document, xmlConfig50, xmlDocBase64, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                // Fallo en la capa de transporte después de agotar los reintentos de Polly
                return new PacIntegrationResult
                {
                    IsSuccess = false,
                    NewState = CfdiState.Rejected,
                    IsHttpError = true,
                    ErrorMessage = $"Fallo de conexión persistente con el PAC {pacId}: {ex.Message}"
                };
            }
            catch (TaskCanceledException)
            {
                // Timeout de red (Equivalente al ERROR_INTERNET_TIMEOUT)
                return new PacIntegrationResult
                {
                    IsSuccess = false,
                    NewState = CfdiState.Pending,
                    IsHttpError = true,
                    ErrorMessage = $"Timeout agotado al intentar conectar con el PAC {pacId}."
                };
            }
        }
    }
}
