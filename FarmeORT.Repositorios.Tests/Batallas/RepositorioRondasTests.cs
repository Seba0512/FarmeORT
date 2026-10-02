using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Batallas;
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
    public void ListarDelTorneo_BatallasDeVariasRondas_LasDevuelveOrdenadasPorRondaYIdConSusJugadores()
    {
        // Sin un orden explícito, SQLite devuelve las filas por id y las de una misma ronda en el orden del índice
        // (acá, del id mayor al menor). Así el test solo pasa si la consulta ordena por ronda y desempata por id.
        _baseDeDatos.Contexto.Database.ExecuteSqlRaw("CREATE INDEX IX_Batallas_Ronda_Prueba ON Batallas (TorneoId, NumeroRonda, Id DESC);");
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var organizador = CrearUsuario(2, Rol.Organizador);
        var torneo = CrearTorneo(4, "COPAAURA", organizador);
        var otroTorneo = CrearTorneo(5, "COPAFARM", organizador);
        _baseDeDatos.Contexto.AddRange(
            new Batalla(torneo, 2, jugadores[0], jugadores[1], new SorteoInicial(jugadores[0])) { Id = 10 },
            new Batalla(torneo, 1, jugadores[0], jugadores[3], new SorteoInicial(jugadores[0])) { Id = 11 },
            new Batalla(torneo, 1, jugadores[1], jugadores[2], new SorteoInicial(jugadores[1])) { Id = 12 },
            new Batalla(otroTorneo, 1, jugadores[0], jugadores[1], new SorteoInicial(jugadores[0])) { Id = 13 });
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var batallas = _repositorio.ListarDelTorneo(4);

        CollectionAssert.AreEqual(new[] { 11, 12, 10 }, batallas.Select(b => b.Id).ToArray());
        Assert.AreEqual("usuario7", batallas[0].Jugador1.Nickname);
        Assert.AreEqual("usuario10", batallas[0].Jugador2.Nickname);
    }

    private static Torneo CrearTorneo()
    {
        return CrearTorneo(4, "COPAAURA", CrearUsuario(2, Rol.Organizador));
    }

    private static Torneo CrearTorneo(int id, string codigo, Usuario organizador)
    {
        return new Torneo(codigo, "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, organizador, FechaActual)
        {
            Id = id,
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
