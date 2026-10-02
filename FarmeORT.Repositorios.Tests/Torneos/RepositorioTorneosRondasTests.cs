using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Torneos;

namespace FarmeORT.Repositorios.Tests.Torneos;

[TestClass]
public sealed class RepositorioTorneosRondasTests
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

    // Generar una ronda guarda las batallas nuevas y el estado del torneo en un solo SaveChanges (D-62).
    [TestMethod]
    public void Actualizar_ConBatallasNuevasDeLaRonda_LasGuardaJuntoConElEstadoDelTorneo()
    {
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, CrearUsuario(2, Rol.Organizador), FechaActual)
        {
            Id = 4,
        };
        _baseDeDatos.Contexto.AddRange(jugadores);
        _baseDeDatos.Contexto.Add(torneo);
        _baseDeDatos.Contexto.SaveChanges();
        torneo.Iniciar(4);
        _baseDeDatos.Contexto.Add(new Batalla(torneo, 1, jugadores[0], jugadores[3], new SorteoInicial(jugadores[0])));
        _baseDeDatos.Contexto.Add(new Batalla(torneo, 1, jugadores[1], jugadores[2], new SorteoInicial(jugadores[1])));

        _repositorio.Actualizar(torneo);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(EstadoTorneo.EnCurso, _baseDeDatos.Contexto.Torneos.Single(t => t.Id == 4).Estado);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Batallas.Count(b => b.TorneoId == 4 && b.NumeroRonda == 1));
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
