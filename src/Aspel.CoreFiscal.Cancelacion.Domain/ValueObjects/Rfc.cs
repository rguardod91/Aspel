namespace Aspel.CoreFiscal.Cancelacion.Domain.ValueObjects
{
    /// <summary>
    /// Representa un Registro Federal de Contribuyentes (RFC) sanitizado y validado.
    /// Implementa la lógica de reemplazo de caracteres especiales (Ñ -> @)
    /// necesaria para la integración con los PACs legacy.
    /// </summary>
    public record Rfc
    {
        public string Value { get; init; }

        public Rfc(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El RFC no puede ser nulo o vacío.");

            // Aplicación de reglas de negocio detectadas en UComercioDigitalCancela.cpp
            // Se normaliza la Ñ y otros caracteres de codificación corruptos detectados en el sistema legacy.
            Value = value.Replace("Ñ", "@")
                         .Replace("ñ", "@")
                         .Replace("Ã‘", "@")
                         .Trim()
                         .ToUpperInvariant();
        }

        public override string ToString() => Value;

        // Operadores implícitos para interoperabilidad con Dapper y servicios externos
        public static implicit operator string(Rfc rfc) => rfc.Value;
        public static implicit operator Rfc(string value) => new Rfc(value);
    }
}
