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
        repositorio.Guardar();
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
