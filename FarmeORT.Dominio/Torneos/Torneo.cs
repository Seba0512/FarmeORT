using System.Text.RegularExpressions;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Torneos;

public sealed class Torneo
{
    private const string PatronCodigo = "^[a-zA-Z0-9]{5,20}$";

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
        _ = fechaActual;
        ValidarCodigo(codigo);

        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Cupo = cupo;
        Organizador = organizador;
        OrganizadorId = organizador.Id;
        Estado = EstadoTorneo.Abierto;
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

    private static void ValidarCodigo(string codigo)
    {
        if(!Regex.IsMatch(codigo, PatronCodigo))
        {
            throw new DatosInvalidosException("El código debe ser alfanumérico, sin espacios, de 5 a 20 caracteres.");
        }
    }
}
