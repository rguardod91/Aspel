namespace Aspel.CoreFiscal.Cancelacion.Application.DTOs
{
    public class CancellationResultDto
    {
        public bool IsSuccess { get; init; }
        public string Uuid { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Acuse { get; init; } = string.Empty;
        public string CfdiState { get; init; } = string.Empty;
        public int PacIdUsed { get; init; }
    }
}
