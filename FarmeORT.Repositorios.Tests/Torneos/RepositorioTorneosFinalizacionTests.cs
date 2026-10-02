using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Torneos;

[TestClass]
public sealed class RepositorioTorneosFinalizacionTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioTorneos _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioTorneos(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    // El campeón no se borra en cascada ni deja el torneo sin campeón: un usuario con historial se da de baja lógica (D-27).
    [TestMethod]
    public void Actualizar_TorneoFinalizado_GuardaAlCampeonConUnaClaveForaneaRestringida()
    {
        var campeon = CrearUsuario(7, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, CrearUsuario(2, Rol.Organizador), FechaActual)
        {
            Id = 4,
        };
        _baseDeDatos.Contexto.AddRange(campeon, torneo);
        _baseDeDatos.Contexto.SaveChanges();
        torneo.Iniciar(4);
        torneo.Finalizar(campeon);

        _repositorio.Actualizar(torneo);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var guardado = _baseDeDatos.Contexto.Torneos.Single(t => t.Id == 4);
        Assert.AreEqual(EstadoTorneo.Finalizado, guardado.Estado);
        Assert.AreEqual(7, guardado.CampeonId);
        var relacion = _baseDeDatos.Contexto.Model.FindEntityType(typeof(Torneo))?.FindNavigation(nameof(Torneo.Campeon))?.ForeignKey;
        Assert.AreEqual(DeleteBehavior.Restrict, relacion?.DeleteBehavior);
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}", new DateOnly(2000, 1, 15), rol, "hash", FechaActual)
        {
            Id = id,
        };
    }
}
