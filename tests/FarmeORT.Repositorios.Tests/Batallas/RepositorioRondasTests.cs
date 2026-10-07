using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
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
    [DataRow(1)]
    [DataRow(2)]
    public void GuardarRonda_DosPedidosPreparanLaMismaRonda_SoloUnoSeConfirma(int numero)
    {
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var torneo = CrearTorneo();
        _baseDeDatos.Contexto.AddRange(jugadores);
        _baseDeDatos.Contexto.Add(torneo);
        if(numero == 2)
        {
            torneo.Iniciar(4);
            var anterior1 = new Batalla(torneo, 1, jugadores[0], jugadores[3], new SorteoInicial(jugadores[0]));
            var anterior2 = new Batalla(torneo, 1, jugadores[1], jugadores[2], new SorteoInicial(jugadores[1]));
            anterior1.Cancelar();
            anterior2.Cancelar();
            _baseDeDatos.Contexto.AddRange(anterior1, anterior2);
        }

        _baseDeDatos.Contexto.SaveChanges();
        using var primero = _baseDeDatos.CrearContexto();
        using var segundo = _baseDeDatos.CrearContexto();
        var torneo1 = primero.Torneos.Single(t => t.Id == torneo.Id);
        var torneo2 = segundo.Torneos.Single(t => t.Id == torneo.Id);
        if(numero == 1)
        {
            torneo1.Iniciar(4);
            torneo2.Iniciar(4);
        }

        var cantidad = numero == 1 ? 4 : 2;
        var jugadores1 = primero.Usuarios.Where(u => u.Rol == Rol.Jugador).OrderBy(u => u.Id).Take(cantidad).ToList();
        var jugadores2 = segundo.Usuarios.Where(u => u.Rol == Rol.Jugador).OrderBy(u => u.Id).Take(cantidad).ToList();
        var ronda1 = CrearBatallas(torneo1, numero, jugadores1);
        var ronda2 = CrearBatallas(torneo2, numero, jugadores2);
        new RepositorioBatallas(primero).GuardarRonda(torneo1, ronda1);
        var repositorio = new RepositorioBatallas(segundo);

        Assert.ThrowsExactly<RondaSinResolverException>(() => repositorio.GuardarRonda(torneo2, ronda2));

        Assert.AreEqual(cantidad / 2, _baseDeDatos.Contexto.Batallas.Count(b => b.TorneoId == torneo.Id && b.NumeroRonda == numero));
        var participantes = _baseDeDatos.Contexto.Batallas.Where(b => b.TorneoId == torneo.Id && b.NumeroRonda == numero)
            .Select(b => new { b.Jugador1Id, b.Jugador2Id }).ToList().SelectMany(b => new[] { b.Jugador1Id, b.Jugador2Id }).ToList();
        Assert.AreEqual(cantidad, participantes.Distinct().Count());
        Assert.AreEqual(EstadoTorneo.EnCurso, _baseDeDatos.Contexto.Torneos.AsNoTracking().Single(t => t.Id == torneo.Id).Estado);
        Assert.IsEmpty(segundo.ChangeTracker.Entries().ToList());
    }

    [TestMethod]
    public void GuardarRonda_TorneoCanceladoPorOtroPedido_NoDejaBatallasNiCambiaLaCancelacion()
    {
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var torneo = CrearTorneo();
        _baseDeDatos.Contexto.AddRange(jugadores);
        _baseDeDatos.Contexto.Add(torneo);
        _baseDeDatos.Contexto.SaveChanges();
        torneo.Iniciar(4);
        var ronda = CrearBatallas(torneo, 1, jugadores);
        using var otro = _baseDeDatos.CrearContexto();
        otro.Torneos.Single(t => t.Id == torneo.Id).Cancelar();
        otro.SaveChanges();

        Assert.ThrowsExactly<TorneoModificadoException>(() => _repositorio.GuardarRonda(torneo, ronda));

        Assert.AreEqual(0, _baseDeDatos.Contexto.Batallas.Count(b => b.TorneoId == torneo.Id));
        Assert.AreEqual(EstadoTorneo.Cancelado, _baseDeDatos.Contexto.Torneos.AsNoTracking().Single(t => t.Id == torneo.Id).Estado);
        Assert.IsEmpty(_baseDeDatos.Contexto.ChangeTracker.Entries().ToList());
    }

    [TestMethod]
    public void GuardarRonda_RondaNueva_GuardaLasBatallasYElEstadoDelTorneoJuntos()
    {
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var torneo = CrearTorneo();
        _baseDeDatos.Contexto.AddRange(jugadores);
        _baseDeDatos.Contexto.Add(torneo);
        _baseDeDatos.Contexto.SaveChanges();
        torneo.Iniciar(4);
        IReadOnlyList<Batalla> ronda =
        [
            new Batalla(torneo, 1, jugadores[0], jugadores[3], new SorteoInicial(jugadores[0])),
            new Batalla(torneo, 1, jugadores[1], jugadores[2], new SorteoInicial(jugadores[1])),
        ];

        _repositorio.GuardarRonda(torneo, ronda);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(EstadoTorneo.EnCurso, _baseDeDatos.Contexto.Torneos.Single(t => t.Id == 4).Estado);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Batallas.Count(b => b.TorneoId == 4 && b.NumeroRonda == 1));
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

    // Empareja al primero con el último, al segundo con el penúltimo, y así.
    private static List<Batalla> CrearBatallas(Torneo torneo, int numero, List<Usuario> jugadores)
    {
        List<Batalla> batallas = [];
        for(var i = 0; i < jugadores.Count / 2; i++)
        {
            var rival = jugadores[jugadores.Count - 1 - i];
            batallas.Add(new Batalla(torneo, numero, jugadores[i], rival, new SorteoInicial(jugadores[i])));
        }

        return batallas;
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
