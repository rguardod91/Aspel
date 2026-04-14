namespace Aspel.CoreFiscal.Cancelacion.Domain.Exceptions
{
    /// <summary>
    /// Lanzada cuando el balanceador de carga no encuentra ningún PAC disponible
    /// ni siquiera el PAC de respaldo por defecto.
    /// </summary>
    public class PacNotAvailableException : CoreFiscalDomainException
    {
        public PacNotAvailableException(string message) : base(message)
        {
        }
    }
}
