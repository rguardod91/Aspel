using Aspel.CoreFiscal.Cancelacion.Domain.Enums;

namespace Aspel.CoreFiscal.Cancelacion.Domain.Entities
{
    /// <summary>
    /// Representa un nodo de proveedor (PAC) en el clúster de balanceo.
    /// Reemplaza la lógica de EstadisticasPac y la gestión de hilos manual[cite: 19, 21].
    /// </summary>
    public class PacNode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AssignedPercentage { get; set; } // Prioridad definida
        public int CurrentScore { get; set; }       // Puntaje acumulado (Balanceo)

        public PenaltySeverity Severity { get; private set; } = PenaltySeverity.None;
        public DateTimeOffset? PenaltyUntil { get; private set; }

        public bool IsSuspended => PenaltyUntil.HasValue && PenaltyUntil > DateTimeOffset.UtcNow;

        public void ApplyPenalty(PenaltySeverity severity, TimeSpan duration)
        {
            Severity = severity;
            PenaltyUntil = DateTimeOffset.UtcNow.Add(duration);
        }

        public void Reset()
        {
            Severity = PenaltySeverity.None;
            PenaltyUntil = null;
        }
    }
}
