using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Torneos;

namespace FarmeORT.Repositorios.Tests.Torneos;

[TestClass]
public sealed class RepositorioTorneosTests
{
    private const string Descripcion = "Torneo eliminatorio de batallas de aura.";

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateOnly FechaInicio = new(2026, 11, 1);
    private static readonly DateOnly FechaFin = new(2026, 11, 30);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioTorneos _repositorio = null!;
    private Usuario _organizador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioTorneos(_baseDeDatos.Contexto);
        _organizador = GuardarOrganizador();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_TorneoNuevo_QuedaGuardadoConSuIdYSuOrganizador()
    {
        var torneo = CrearTorneo("COPAAURA");

        _repositorio.Agregar(torneo);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsGreaterThan(0, torneo.Id);
        var guardado = _baseDeDatos.Contexto.Torneos.SingleOrDefault(t => t.Id == torneo.Id);
        Assert.IsNotNull(guardado);
        Assert.AreEqual("COPAAURA", guardado.Codigo);
        Assert.AreEqual("Copa Aura", guardado.Nombre);
        Assert.AreEqual(Descripcion, guardado.Descripcion);
        Assert.AreEqual(FechaInicio, guardado.FechaInicio);
        Assert.AreEqual(FechaFin, guardado.FechaFin);
        Assert.AreEqual(8, guardado.Cupo);
        Assert.AreEqual(EstadoTorneo.Abierto, guardado.Estado);
        Assert.AreEqual(_organizador.Id, guardado.OrganizadorId);
    }

    private Torneo CrearTorneo(string codigo, string nombre = "Copa Aura", DateOnly? fechaInicio = null)
    {
        var inicio = fechaInicio ?? FechaInicio;
        return new Torneo(codigo, nombre, Descripcion, inicio, inicio.AddDays(29), 8, _organizador, FechaActual);
    }

    private Usuario GuardarOrganizador()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(organizador);
        _baseDeDatos.Contexto.SaveChanges();
        return organizador;
    }
}
