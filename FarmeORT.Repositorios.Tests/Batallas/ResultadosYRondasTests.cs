using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Elo;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.Repositorios.Batallas;
using FarmeORT.Repositorios.Inscripciones;
using FarmeORT.Repositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Batallas;

[TestClass]
public sealed class ResultadosYRondasTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 6);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    [DataRow(4, 1, 1)]
    [DataRow(4, 1, 2)]
    [DataRow(8, 1, 1)]
    [DataRow(8, 1, 2)]
    [DataRow(8, 2, 1)]
    [DataRow(8, 2, 2)]
    public void GuardarRonda_FallosPosterioresALaLecturaDeAceptadas_RechazaSinBatallasParciales(int cupo, int rondaAnterior, int fallosPendientes)
    {
        var torneoId = PrepararTorneo(cupo, rondaAnterior);
        ResolverSalvoUltimas(torneoId, rondaAnterior, fallosPendientes);
        using var generacion = _baseDeDatos.CrearContexto();
        using var fallos = _baseDeDatos.CrearContexto();
        var torneo = new RepositorioTorneos(generacion).ObtenerPorId(torneoId)!;
        var jugadoresAnteriores = new RepositorioInscripciones(generacion).ListarAceptadasDelTorneo(torneoId).Select(i => i.Jugador).ToList();
        var pendientes = fallos.Batallas.Where(b => b.TorneoId == torneoId && b.NumeroRonda == rondaAnterior && b.Estado == EstadoBatalla.Pendiente)
            .OrderBy(b => b.Id).Select(b => b.Id).ToList();
        foreach(var batallaId in pendientes)
        {
            Fallar(fallos, batallaId);
        }

        var repositorio = new RepositorioBatallas(generacion);
        Assert.IsTrue(repositorio.ListarDelTorneo(torneoId).Where(b => b.NumeroRonda == rondaAnterior).All(b => b.Estado == EstadoBatalla.Finalizada));
        var nuevas = CrearBatallas(torneo, rondaAnterior + 1, jugadoresAnteriores);

        Assert.ThrowsExactly<TorneoModificadoException>(() => repositorio.GuardarRonda(torneo, nuevas));

        Assert.AreEqual(0, _baseDeDatos.Contexto.Batallas.Count(b => b.TorneoId == torneoId && b.NumeroRonda == rondaAnterior + 1));
        Assert.AreEqual(cupo / (1 << rondaAnterior), _baseDeDatos.Contexto.Inscripciones.Count(i => i.TorneoId == torneoId && i.Estado == EstadoInscripcion.Aceptada));
        Assert.AreEqual(EstadoTorneo.EnCurso, _baseDeDatos.Contexto.Torneos.AsNoTracking().Single(t => t.Id == torneoId).Estado);
        Assert.IsEmpty(generacion.ChangeTracker.Entries().ToList());
    }

    [TestMethod]
    [DataRow(4, 1)]
    [DataRow(8, 1)]
    [DataRow(8, 2)]
    public void GuardarRonda_FallosAntesDeLaGeneracion_IncluyeSoloSupervivientes(int cupo, int rondaAnterior)
    {
        var torneoId = PrepararTorneo(cupo, rondaAnterior);
        ResolverSalvoUltimas(torneoId, rondaAnterior, 0);
        using var generacion = _baseDeDatos.CrearContexto();
        var torneo = new RepositorioTorneos(generacion).ObtenerPorId(torneoId)!;
        var anteriores = new RepositorioBatallas(generacion).ListarDelTorneo(torneoId);
        Assert.IsTrue(anteriores.All(b => b.Estado == EstadoBatalla.Finalizada));
        var jugadores = new RepositorioInscripciones(generacion).ListarAceptadasDelTorneo(torneoId).Select(i => i.Jugador).ToList();
        var nuevas = CrearBatallas(torneo, rondaAnterior + 1, jugadores);

        new RepositorioBatallas(generacion).GuardarRonda(torneo, nuevas);

        var participantes = _baseDeDatos.Contexto.Batallas.Where(b => b.TorneoId == torneoId && b.NumeroRonda == rondaAnterior + 1)
            .Select(b => new { b.Jugador1Id, b.Jugador2Id }).ToList().SelectMany(b => new[] { b.Jugador1Id, b.Jugador2Id }).ToArray();
        var supervivientes = _baseDeDatos.Contexto.Inscripciones.Where(i => i.TorneoId == torneoId && i.Estado == EstadoInscripcion.Aceptada).Select(i => i.JugadorId).ToArray();
        var eliminados = _baseDeDatos.Contexto.Inscripciones.Where(i => i.TorneoId == torneoId && i.Estado == EstadoInscripcion.Eliminada).Select(i => i.JugadorId).ToArray();
        CollectionAssert.AreEquivalent(supervivientes, participantes);
        Assert.HasCount(cupo / (1 << rondaAnterior), participantes);
        Assert.IsFalse(participantes.Intersect(eliminados).Any());
        CollectionAssert.AllItemsAreUnique(participantes);
    }

    [TestMethod]
    [DataRow(4, 1)]
    [DataRow(8, 2)]
    public void Guardar_DosFallosDistintosDelMismoTorneo_ConflictoSinEloNiEliminacionParcial(int cupo, int numero)
    {
        var torneoId = PrepararTorneo(cupo, numero);
        var ids = _baseDeDatos.Contexto.Batallas.Where(b => b.TorneoId == torneoId && b.NumeroRonda == numero).OrderBy(b => b.Id).Select(b => b.Id).ToArray();
        using var primero = _baseDeDatos.CrearContexto();
        using var segundo = _baseDeDatos.CrearContexto();
        var batalla1 = PrepararFallo(primero, ids[0]);
        var batalla2 = PrepararFallo(segundo, ids[1]);
        var eloPrevio = 1200 + (16 * (numero - 1));
        new RepositorioBatallas(primero).Guardar();

        Assert.ThrowsExactly<BatallaModificadaException>(() => new RepositorioBatallas(segundo).Guardar());

        var guardada1 = _baseDeDatos.Contexto.Batallas.AsNoTracking().Single(b => b.Id == batalla1.Id);
        var guardada2 = _baseDeDatos.Contexto.Batallas.AsNoTracking().Single(b => b.Id == batalla2.Id);
        Assert.AreEqual(EstadoBatalla.Finalizada, guardada1.Estado);
        Assert.AreEqual(16, guardada1.CambioEloJugador1);
        Assert.AreEqual(-16, guardada1.CambioEloJugador2);
        Assert.AreEqual(eloPrevio + 16, _baseDeDatos.Contexto.Usuarios.AsNoTracking().Single(u => u.Id == batalla1.Jugador1Id).Elo);
        Assert.AreEqual(eloPrevio - 16, _baseDeDatos.Contexto.Usuarios.AsNoTracking().Single(u => u.Id == batalla1.Jugador2Id).Elo);
        Assert.AreEqual(EstadoInscripcion.Eliminada, _baseDeDatos.Contexto.Inscripciones.Single(i => i.TorneoId == torneoId && i.JugadorId == batalla1.Jugador2Id).Estado);
        Assert.AreEqual(EstadoBatalla.Pendiente, guardada2.Estado);
        Assert.IsNull(guardada2.GanadorId);
        Assert.IsNull(guardada2.Comentario);
        Assert.IsNull(guardada2.CambioEloJugador1);
        Assert.IsNull(guardada2.CambioEloJugador2);
        Assert.AreEqual(eloPrevio, _baseDeDatos.Contexto.Usuarios.AsNoTracking().Single(u => u.Id == batalla2.Jugador1Id).Elo);
        Assert.AreEqual(eloPrevio, _baseDeDatos.Contexto.Usuarios.AsNoTracking().Single(u => u.Id == batalla2.Jugador2Id).Elo);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _baseDeDatos.Contexto.Inscripciones.Single(i => i.TorneoId == torneoId && i.JugadorId == batalla2.Jugador2Id).Estado);
        Assert.IsEmpty(segundo.ChangeTracker.Entries().ToList());

        // Un pedido nuevo vuelve a leer la versión y puede confirmar el fallo que perdió el conflicto.
        Fallar(segundo, batalla2.Id);
        Assert.AreEqual(EstadoBatalla.Finalizada, segundo.Batallas.AsNoTracking().Single(b => b.Id == batalla2.Id).Estado);
    }

    private int PrepararTorneo(int cupo, int rondaAnterior)
    {
        var organizador = CrearUsuario(2, Rol.Organizador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), cupo, organizador, FechaActual);
        var jugadores = Enumerable.Range(7, cupo).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        _baseDeDatos.Contexto.Inscripciones.AddRange(jugadores.Select(j => new Inscripcion(torneo, j, FechaActual.ToDateTime(TimeOnly.MinValue))));
        torneo.Iniciar(cupo);
        _baseDeDatos.Contexto.Batallas.AddRange(CrearBatallas(torneo, 1, jugadores));
        _baseDeDatos.Contexto.SaveChanges();
        if(rondaAnterior == 2)
        {
            ResolverSalvoUltimas(torneo.Id, 1, 0);
            _baseDeDatos.Contexto.ChangeTracker.Clear();
            torneo = _baseDeDatos.Contexto.Torneos.Single(t => t.Id == torneo.Id);
            jugadores = _baseDeDatos.Contexto.Inscripciones.Where(i => i.TorneoId == torneo.Id && i.Estado == EstadoInscripcion.Aceptada)
                .Include(i => i.Jugador).OrderBy(i => i.JugadorId).Select(i => i.Jugador).ToList();
            _baseDeDatos.Contexto.Batallas.AddRange(CrearBatallas(torneo, 2, jugadores));
            _baseDeDatos.Contexto.SaveChanges();
        }

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return torneo.Id;
    }

    private void ResolverSalvoUltimas(int torneoId, int numero, int pendientes)
    {
        using var contexto = _baseDeDatos.CrearContexto();
        var ids = contexto.Batallas.Where(b => b.TorneoId == torneoId && b.NumeroRonda == numero).OrderBy(b => b.Id).Select(b => b.Id).ToList();
        foreach(var id in ids.Take(ids.Count - pendientes))
        {
            Fallar(contexto, id);
        }
    }

    private static void Fallar(FarmeORTContext contexto, int batallaId)
    {
        PrepararFallo(contexto, batallaId);
        new RepositorioBatallas(contexto).Guardar();
    }

    private static Batalla PrepararFallo(FarmeORTContext contexto, int batallaId)
    {
        var repositorio = new RepositorioBatallas(contexto);
        var batalla = repositorio.ObtenerPorId(batallaId)!;
        var inscripcion = new RepositorioInscripciones(contexto).ObtenerAceptada(batalla.TorneoId, batalla.Jugador2Id)!;
        var resultado = new CalculadorEloBase().Calcular(batalla.Jugador1.Elo, batalla.Jugador2.Elo);
        var cambio1 = resultado.NuevoEloGanador - batalla.Jugador1.Elo;
        var cambio2 = resultado.NuevoEloPerdedor - batalla.Jugador2.Elo;
        batalla.Jugador1.ActualizarElo(resultado.NuevoEloGanador);
        batalla.Jugador2.ActualizarElo(resultado.NuevoEloPerdedor);
        batalla.Fallar(batalla.Jugador1Id, "Victoria por abandono.", cambio1, cambio2);
        inscripcion.Eliminar();
        return batalla;
    }

    private static List<Batalla> CrearBatallas(Torneo torneo, int numero, List<Usuario> jugadores)
    {
        List<Batalla> batallas = [];
        for(var i = 0; i < jugadores.Count / 2; i++)
        {
            batallas.Add(new Batalla(torneo, numero, jugadores[i], jugadores[jugadores.Count - 1 - i], new SorteoInicial()));
        }

        return batallas;
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}", new DateOnly(2000, 1, 15), rol, "hash", FechaActual) { Id = id };
    }

    private sealed class SorteoInicial : ISorteo
    {
        public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
        {
            return jugador1;
        }
    }
}
