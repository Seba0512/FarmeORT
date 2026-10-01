using System.Text.RegularExpressions;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Torneos;

public sealed class Torneo
{
    private const string PatronCodigo = "^[a-zA-Z0-9]{5,20}$";
    private const int LargoMinimoNombre = 5;
    private const int LargoMaximoNombre = 60;
    private const int LargoMinimoDescripcion = 20;
    private const int LargoMaximoDescripcion = 500;
    private const int CupoMinimo = 4;
    private const int CupoMaximo = 64;

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
        ValidarNombre(nombre);
        ValidarDescripcion(descripcion);
        ValidarCupo(cupo);

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

    private static void ValidarNombre(string nombre)
    {
        if(string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosInvalidosException("El nombre es obligatorio.");
        }

        if(nombre.Length < LargoMinimoNombre || nombre.Length > LargoMaximoNombre)
        {
            throw new DatosInvalidosException("El nombre debe tener entre 5 y 60 caracteres.");
        }
    }

    private static void ValidarDescripcion(string descripcion)
    {
        if(string.IsNullOrWhiteSpace(descripcion))
        {
            throw new DatosInvalidosException("La descripción es obligatoria.");
        }

        if(descripcion.Length < LargoMinimoDescripcion || descripcion.Length > LargoMaximoDescripcion)
        {
            throw new DatosInvalidosException("La descripción debe tener entre 20 y 500 caracteres.");
        }
    }

    // El torneo es eliminatorio: con una potencia de 2 cada ronda empareja a todos los jugadores que siguen.
    private static void ValidarCupo(int cupo)
    {
        if(cupo < CupoMinimo || cupo > CupoMaximo || !int.IsPow2(cupo))
        {
            throw new DatosInvalidosException("El cupo debe ser una potencia de 2 entre 4 y 64.");
        }
    }
}
