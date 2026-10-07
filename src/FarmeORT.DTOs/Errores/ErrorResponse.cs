namespace FarmeORT.DTOs.Errores;

public sealed class ErrorResponse
{
    public string UserMessage { get; init; } = string.Empty;

    public string ErrorCode { get; init; } = string.Empty;

    public string? MoreInfo { get; init; }

    public IReadOnlyList<string>? Detalles { get; init; }
}
