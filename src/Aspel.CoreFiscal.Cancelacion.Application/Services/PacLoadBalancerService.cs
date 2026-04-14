using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using Aspel.CoreFiscal.Cancelacion.Domain.Exceptions;
using Aspel.CoreFiscal.Cancelacion.Domain.Services;

namespace Aspel.CoreFiscal.Cancelacion.Application.Services
{
    public class PacLoadBalancerService : IPacLoadBalancerService
    {
        private readonly IPacStateRepository _pacRepository;

        // Tiempos heredados de la lógica de C++ (iTiempoCastigoMenor, Medio, Mayor)
        private const int MinorPenaltyMs = 5000;
        private const int MediumPenaltyMs = 9000;
        private const int MajorPenaltyMs = 70000;

        public PacLoadBalancerService(IPacStateRepository pacRepository)
        {
            _pacRepository = pacRepository;
        }

        public async Task<PacNode> GetBestAvailablePacAsync(int defaultPacId, CancellationToken cancellationToken)
        {
            var pacs = await _pacRepository.GetAllPacsAsync(cancellationToken);
            var availablePacs = pacs.Where(p => !p.IsSuspended).ToList();

            if (!availablePacs.Any())
            {
                // Si todos están suspendidos, aplicamos el comportamiento de C++: usar el PAC por defecto.
                var defaultPac = await _pacRepository.GetPacByIdAsync(defaultPacId, cancellationToken);
                if (defaultPac == null) throw new PacNotAvailableException("No hay PACs disponibles y el PAC por defecto no existe.");
                return defaultPac;
            }

            var percentages = availablePacs.Select(p => p.AssignedPercentage);
            int mcm = McmCalculator.Calculate(percentages);

            // Si el PAC con menor puntaje ya alcanzó el MCM, reiniciamos los puntajes (Lógica legacy adaptada)
            var bestPac = availablePacs.OrderBy(p => p.CurrentScore).First();
            if (bestPac.CurrentScore >= mcm && mcm > 0)
            {
                foreach (var pac in availablePacs)
                {
                    pac.CurrentScore = 0;
                    await _pacRepository.UpdatePacAsync(pac, cancellationToken);
                }
            }

            // Actualizamos el puntaje del PAC seleccionado para el siguiente ciclo
            if (bestPac.AssignedPercentage > 0)
            {
                bestPac.CurrentScore += (mcm / bestPac.AssignedPercentage);
                await _pacRepository.UpdatePacAsync(bestPac, cancellationToken);
            }

            return bestPac;
        }

        public async Task ReportPacPerformanceAsync(int pacId, long durationMs, bool isHttpError, CancellationToken cancellationToken)
        {
            var pac = await _pacRepository.GetPacByIdAsync(pacId, cancellationToken);
            if (pac == null || pac.IsSuspended) return;

            bool stateChanged = false;

            if (isHttpError)
            {
                pac.ApplyPenalty(PenaltySeverity.Major, TimeSpan.FromMilliseconds(MajorPenaltyMs));
                stateChanged = true;
            }
            else if (durationMs > MediumPenaltyMs)
            {
                pac.ApplyPenalty(PenaltySeverity.Major, TimeSpan.FromMilliseconds(MajorPenaltyMs));
                stateChanged = true;
            }
            else if (durationMs > MinorPenaltyMs)
            {
                pac.ApplyPenalty(PenaltySeverity.Medium, TimeSpan.FromMilliseconds(MediumPenaltyMs));
                stateChanged = true;
            }

            if (stateChanged)
            {
                await _pacRepository.UpdatePacAsync(pac, cancellationToken);
            }
        }
    }
}
