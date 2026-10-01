namespace FarmeORT.DTOs.Torneos;

// El detalle (TorneoDetalleResponse) extiende este resumen con los datos del cuadro.
public class TorneoResponse
{
    public int Id { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;

    public DateOnly FechaInicio { get; init; }

    public DateOnly FechaFin { get; init; }

    public int Cupo { get; init; }

    public int InscripcionesAceptadas { get; init; }

    public int OrganizadorId { get; init; }

    public string Organizador { get; init; } = string.Empty;
}
