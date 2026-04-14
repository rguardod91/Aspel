using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using Aspel.CoreFiscal.Cancelacion.Domain.ValueObjects;

namespace Aspel.CoreFiscal.Cancelacion.Domain.Entities
{
    /// <summary>
    /// Entidad principal que representa un comprobante (CFDI o Retención) en el flujo de cancelación.
    /// Reemplaza la estructura TDatosDocumento del sistema C++[cite: 1, 2].
    /// </summary>
    public class CfdiDocument
    {
        public int Id { get; set; } = -1;
        public string Uuid { get; set; } = string.Empty;

        // Uso de Value Object para garantizar integridad de datos
        public Rfc RfcEmisor { get; set; } = null!;
        public Rfc RfcReceptor { get; set; } = null!;

        public decimal Total { get; set; }
        public CfdiState State { get; set; } = CfdiState.Undefined;
        public string Acuse { get; set; } = string.Empty;
        public int VersionCfdi { get; set; }

        // Propiedades de ruteo y telemetría (SLA)
        public int PacId { get; set; } = -1;
        public long DurationPacMs { get; set; }
        public bool IsHttpError { get; set; }
    }
}
