using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Batallas;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Batallas;

[TestClass]
public sealed class RepositorioBatallasTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateTimeOffset FechaLanzamiento = new(2026, 10, 1, 16, 30, 0, TimeSpan.Zero);

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
    public void ObtenerPorId_BatallaInexistente_DevuelveNull()
    {
        var resultado = _repositorio.ObtenerPorId(99);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerPorId_BatallaExistente_CargaTorneoJugadoresYSecuenciaOrdenada()
    {
        var batalla = GuardarBatallaConHistorial();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.ObtenerPorId(batalla.Id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(12, resultado.Id);
        Assert.AreEqual(2, resultado.NumeroRonda);
        Assert.IsNotNull(resultado.Torneo);
        Assert.AreEqual("COPAAURA", resultado.Torneo.Codigo);
        Assert.IsNotNull(resultado.Torneo.Organizador);
        Assert.AreEqual(2, resultado.Torneo.Organizador.Id);
        Assert.IsNotNull(resultado.Jugador1);
        Assert.AreEqual("usuario7", resultado.Jugador1.Nickname);
        Assert.AreEqual(1200, resultado.Jugador1.Elo);
        Assert.IsNotNull(resultado.Jugador2);
        Assert.AreEqual("usuario8", resultado.Jugador2.Nickname);
        Assert.AreEqual(1200, resultado.Jugador2.Elo);
        Assert.AreEqual(7, resultado.JugadorInicialId);
        Assert.AreEqual(7, resultado.JugadorTurnoId);
        Assert.HasCount(2, resultado.Lanzamientos);
        CollectionAssert.AreEqual(new[] { 1, 2 }, resultado.Lanzamientos.Select(l => l.Turno).ToArray());
        var primero = resultado.Lanzamientos.First();
        Assert.AreEqual(FechaLanzamiento, primero.Fecha);
        Assert.AreEqual(7, primero.Jugador.Id);
        Assert.AreEqual("SIXSEVEN", primero.Skill.Codigo);
    }

    [TestMethod]
    public void Guardar_LanzamientoYCambioDeTurno_PersisteAmbosCambios()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 3, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));
        _baseDeDatos.Contexto.Entry(batalla).Property(b => b.JugadorTurnoId).CurrentValue = 8;

        _repositorio.Guardar();

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var guardada = _baseDeDatos.Contexto.Batallas.Single(b => b.Id == batalla.Id);
        Assert.AreEqual(8, guardada.JugadorTurnoId);
        Assert.AreEqual(EstadoBatalla.Pendiente, guardada.Estado);
        var lanzamientos = _baseDeDatos.Contexto.Lanzamientos.Where(l => l.BatallaId == batalla.Id).OrderBy(l => l.Turno).ToList();
        Assert.HasCount(3, lanzamientos);
        Assert.AreEqual(3, lanzamientos.Last().Turno);
        Assert.AreEqual(7, lanzamientos.Last().JugadorId);
        Assert.AreEqual(4, lanzamientos.Last().SkillId);
    }

    [TestMethod]
    public void Persistencia_TurnoDuplicado_RechazaSinGuardarLanzamientoNiCambioDeTurno()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 2, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));
        _baseDeDatos.Contexto.Entry(batalla).Property(b => b.JugadorTurnoId).CurrentValue = 8;

        Assert.ThrowsExactly<DbUpdateException>(() => _baseDeDatos.Contexto.SaveChanges());

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(7, _baseDeDatos.Contexto.Batallas.Single(b => b.Id == batalla.Id).JugadorTurnoId);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
        Assert.IsFalse(_baseDeDatos.Contexto.Skills.Any(s => s.Id == 4));
    }

    private Batalla GuardarBatallaConHistorial()
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, CrearUsuario(2, Rol.Organizador), FechaActual) { Id = 5 };
        var batalla = new Batalla(torneo, 2, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = 12 };
        var skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2) { Id = 3 };
        _baseDeDatos.Contexto.Batallas.Add(batalla);
        _baseDeDatos.Contexto.Lanzamientos.AddRange(
            new Lanzamiento(batalla, 2, jugador2, skill, FechaLanzamiento.AddMinutes(1)),
            new Lanzamiento(batalla, 1, jugador1, skill, FechaLanzamiento));
        _baseDeDatos.Contexto.SaveChanges();
        return batalla;
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual) { Id = id };
    }

    private sealed class SorteoInicial(Usuario elegido) : ISorteo
    {
        public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
        {
            return elegido;
        }
    }
}
