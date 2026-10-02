using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Batallas;
using FarmeORT.Repositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Batallas;

[TestClass]
public sealed class RepositorioRondasTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioBatallas _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioBatallas(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_BatallaNueva_QuedaRastreadaSinGuardarseTodavia()
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = CrearTorneo();
        _baseDeDatos.Contexto.AddRange(jugador1, jugador2, torneo);
        _baseDeDatos.Contexto.SaveChanges();
        var batalla = new Batalla(torneo, 1, jugador1, jugador2, new SorteoInicial(jugador1));

        _repositorio.Agregar(batalla);

        Assert.AreEqual(EntityState.Added, _baseDeDatos.Contexto.Entry(batalla).State);
        Assert.AreEqual(0, _baseDeDatos.Contexto.Batallas.Count());
    }

    [TestMethod]
    public void Agregar_ConElTorneoActualizado_GuardaLasBatallasYElEstadoJuntos()
    {
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var torneo = CrearTorneo();
        _baseDeDatos.Contexto.AddRange(jugadores);
        _baseDeDatos.Contexto.Add(torneo);
        _baseDeDatos.Contexto.SaveChanges();
        torneo.Iniciar(4);
        _repositorio.Agregar(new Batalla(torneo, 1, jugadores[0], jugadores[3], new SorteoInicial(jugadores[0])));
        _repositorio.Agregar(new Batalla(torneo, 1, jugadores[1], jugadores[2], new SorteoInicial(jugadores[1])));

        new RepositorioTorneos(_baseDeDatos.Contexto).Actualizar(torneo);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(EstadoTorneo.EnCurso, _baseDeDatos.Contexto.Torneos.Single(t => t.Id == 4).Estado);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Batallas.Count(b => b.TorneoId == 4 && b.NumeroRonda == 1));
    }

    private static Torneo CrearTorneo()
    {
        return new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, CrearUsuario(2, Rol.Organizador), FechaActual)
        {
            Id = 4,
        };
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}", new DateOnly(2000, 1, 15), rol, "hash", FechaActual)
        {
            Id = id,
        };
    }

    private sealed class SorteoInicial(Usuario elegido) : ISorteo
    {
        public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
        {
            return elegido;
        }
    }
}
