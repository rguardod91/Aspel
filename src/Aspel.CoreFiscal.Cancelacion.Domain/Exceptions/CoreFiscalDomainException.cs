namespace Aspel.CoreFiscal.Cancelacion.Domain.Exceptions
{
    /// <summary>
    /// Clase base para todas las excepciones de reglas de negocio del dominio.
    /// La capa API usará esta clase base para mapear automáticamente estos errores a HTTP 400 (Bad Request) o 422 (Unprocessable Entity).
    /// </summary>
    public abstract class CoreFiscalDomainException : Exception
    {
        protected CoreFiscalDomainException(string message) : base(message)
        {
        }

        protected CoreFiscalDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
