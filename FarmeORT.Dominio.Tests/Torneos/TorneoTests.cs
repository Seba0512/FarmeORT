using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Tests.Torneos;

[TestClass]
public sealed class TorneoTests
{
    private const string Descripcion = "Torneo eliminatorio de batallas de aura.";

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateOnly FechaInicio = new(2026, 11, 1);
    private static readonly DateOnly FechaFin = new(2026, 11, 30);

    [TestMethod]
    public void Constructor_DatosValidos_ConservaLosDatosYQuedaAbierto()
    {
        var organizador = CrearOrganizador(Rol.Organizador);

        var torneo = new Torneo("COPAAURA", "Copa Aura", Descripcion, FechaInicio, FechaFin, 8, organizador, FechaActual);

        Assert.AreEqual("COPAAURA", torneo.Codigo);
        Assert.AreEqual("Copa Aura", torneo.Nombre);
        Assert.AreEqual(Descripcion, torneo.Descripcion);
        Assert.AreEqual(FechaInicio, torneo.FechaInicio);
        Assert.AreEqual(FechaFin, torneo.FechaFin);
        Assert.AreEqual(8, torneo.Cupo);
        Assert.AreSame(organizador, torneo.Organizador);
        Assert.AreEqual(organizador.Id, torneo.OrganizadorId);
        Assert.AreEqual(EstadoTorneo.Abierto, torneo.Estado);
    }

    private static Usuario CrearOrganizador(Rol rol)
    {
        return new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), rol, "hash", FechaActual)
        {
            Id = 2,
        };
    }
}
