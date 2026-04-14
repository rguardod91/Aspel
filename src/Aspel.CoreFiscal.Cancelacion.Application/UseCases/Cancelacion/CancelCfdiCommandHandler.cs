using Aspel.CoreFiscal.Cancelacion.Application.DTOs;
using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Application.Services;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using Aspel.CoreFiscal.Cancelacion.Domain.ValueObjects;
using MediatR;
using System.Diagnostics;

namespace Aspel.CoreFiscal.Cancelacion.Application.UseCases.Cancelacion
{
    public class CancelCfdiCommandHandler : IRequestHandler<CancelCfdiCommand, CancellationResultDto>
    {
        private readonly ICfdiCancellationRepository _repository;
        private readonly IPacLoadBalancerService _loadBalancer;
        private readonly IPacIntegrationClient _pacClient;

        public CancelCfdiCommandHandler(
            ICfdiCancellationRepository repository,
            IPacLoadBalancerService loadBalancer,
            IPacIntegrationClient pacClient)
        {
            _repository = repository;
            _loadBalancer = loadBalancer;
            _pacClient = pacClient;
        }

        public async Task<CancellationResultDto> Handle(CancelCfdiCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Instanciar Entidad y Value Objects (Sanitización automática de RFCs)
            var document = await _repository.GetByUuidAsync(request.Uuid, cancellationToken)
                           ?? new CfdiDocument
                           {
                               Uuid = request.Uuid,
                               State = CfdiState.New
                           };

            document.RfcEmisor = new Rfc(request.RfcEmisor);
            document.RfcReceptor = new Rfc(request.RfcReceptor);
            document.Total = request.Total;

            // Regla de Negocio: No procesar si ya está cancelado exitosamente
            if (document.State == CfdiState.Canceled)
            {
                return new CancellationResultDto
                {
                    IsSuccess = true,
                    Uuid = document.Uuid,
                    Message = "El documento ya se encuentra cancelado.",
                    Acuse = document.Acuse,
                    CfdiState = document.State.ToString()
                };
            }

            // 2. Obtener el mejor PAC disponible mediante el balanceador MCM
            var bestPac = await _loadBalancer.GetBestAvailablePacAsync(request.PacDefaultId, cancellationToken);
            document.PacId = bestPac.Id;

            // 3. Ejecutar integración externa (El Polly Circuit Breaker actuará dentro del IPacIntegrationClient)
            var integrationResult = await _pacClient.CancelCfdiAsync(
                bestPac.Id,
                document,
                request.XmlConfig50,
                request.XmlDocBase64,
                cancellationToken);

            stopwatch.Stop();
            document.DurationPacMs = stopwatch.ElapsedMilliseconds;
            document.IsHttpError = integrationResult.IsHttpError;
            document.Acuse = integrationResult.Acuse;
            document.State = integrationResult.NewState;

            // 4. Reportar rendimiento para aplicar posibles castigos
            await _loadBalancer.ReportPacPerformanceAsync(
                bestPac.Id,
                document.DurationPacMs,
                document.IsHttpError,
                cancellationToken);

            // 5. Persistir estado en BD (Dapper)
            if (document.Id == -1)
                await _repository.SaveAsync(document, cancellationToken);
            else
                await _repository.UpdateStatusAsync(document, cancellationToken);

            // 6. Retornar resultado estándar
            return new CancellationResultDto
            {
                IsSuccess = integrationResult.IsSuccess,
                Uuid = document.Uuid,
                Message = integrationResult.IsSuccess ? "Cancelación procesada exitosamente." : integrationResult.ErrorMessage,
                Acuse = document.Acuse,
                CfdiState = document.State.ToString(),
                PacIdUsed = bestPac.Id
            };
        }
    }
}
