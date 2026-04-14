namespace Aspel.CoreFiscal.Cancelacion.Domain.Enums
{
    /// <summary>
    /// Representa los estados posibles de un CFDI dentro del flujo de cancelación.
    /// </summary>
    public enum CfdiState
    {
        Undefined = 0,
        New = 1,                 // EstCfdiNuevo
        Pending = 2,             // EstCfdiPendiente
        Canceled = 4,            // EstCfdiTimbrado / EstCfdiFinal
        NeedsAuthorization = 5,  // EstNecesitaAutorizacion
        Rejected = 6             // Solicitud rechazada o fallida
    }
}
