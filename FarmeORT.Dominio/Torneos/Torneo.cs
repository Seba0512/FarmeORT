using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Torneos;

public sealed class Torneo
{
    public Torneo(
        string codigo,
        string nombre,
        string descripcion,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int cupo,
        Usuario organizador,
        DateOnly fechaActual)
    {
        _ = codigo;
        _ = nombre;
        _ = descripcion;
        _ = fechaInicio;
        _ = fechaFin;
        _ = cupo;
        _ = organizador;
        _ = fechaActual;
        throw new NotImplementedException();
    }

    public int Id { get; init; }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public DateOnly FechaInicio { get; private set; }

    public DateOnly FechaFin { get; private set; }

    public int Cupo { get; private set; }

    public EstadoTorneo Estado { get; private set; }

    public int OrganizadorId { get; private set; }

    public Usuario Organizador { get; private set; }
}
