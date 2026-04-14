namespace Aspel.CoreFiscal.Cancelacion.Domain.Enums
{
    /// <summary>
    /// Define la severidad del castigo aplicado a un PAC por bajo rendimiento o errores.
    /// </summary>
    public enum PenaltySeverity
    {
        None = 0,
        Minor = 1,   // PAC_CASTIGOMENOR
        Medium = 2,  // PAC_CASTIGOMEDIO
        Major = 3    // PAC_CASTIGOMAYOR
    }
}
