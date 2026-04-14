namespace Aspel.CoreFiscal.Cancelacion.Domain.Exceptions
{
    /// <summary>
    /// Lanzada cuando se intenta realizar una operación sobre un CFDI cuyo estado actual no lo permite.
    /// Ejemplo: Intentar cancelar un CFDI que ya está en estado 'Canceled'.
    /// </summary>
    public class InvalidCfdiStateException : CoreFiscalDomainException
    {
        public InvalidCfdiStateException(string uuid, string currentState)
            : base($"No se puede procesar la cancelación para el CFDI '{uuid}' porque su estado actual es '{currentState}'.")
        {
        }
    }
}
