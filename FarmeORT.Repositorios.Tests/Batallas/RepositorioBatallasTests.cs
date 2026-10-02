using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
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

    [TestMethod]
    public void Persistencia_SkillRepetidaPorJugador_RechazaElDuplicado()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = _baseDeDatos.Contexto.Skills.Single(s => s.Id == 3);
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 3, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));

        Assert.ThrowsExactly<DbUpdateException>(() => _baseDeDatos.Contexto.SaveChanges());

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(9)]
    public void Persistencia_TurnoFueraDeUnoAOcho_RechazaDatos(int turno)
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, turno, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));

        Assert.ThrowsExactly<DbUpdateException>(() => _baseDeDatos.Contexto.SaveChanges());

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
    }

    [TestMethod]
    public void Persistencia_CancelacionConcurrente_NoGuardaLanzamientoSobreBatallaCancelada()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 3, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));
        _baseDeDatos.Contexto.Entry(batalla).Property(b => b.JugadorTurnoId).CurrentValue = 8;
        using var otroContexto = CrearOtroContexto();
        var cancelada = otroContexto.Batallas.Single(b => b.Id == batalla.Id);
        cancelada.Cancelar();
        otroContexto.SaveChanges();

        Assert.ThrowsExactly<DbUpdateConcurrencyException>(() => _baseDeDatos.Contexto.SaveChanges());

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var guardada = _baseDeDatos.Contexto.Batallas.Single(b => b.Id == batalla.Id);
        Assert.AreEqual(EstadoBatalla.Cancelada, guardada.Estado);
        Assert.IsNull(guardada.JugadorTurnoId);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
        Assert.IsFalse(_baseDeDatos.Contexto.Skills.Any(s => s.Id == 4));
    }

    [TestMethod]
    public void Guardar_TurnoYaGuardado_TraduceConflictoATurnoIncorrecto()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 2, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));

        Assert.ThrowsExactly<TurnoIncorrectoException>(_repositorio.Guardar);

        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
    }

    [TestMethod]
    public void Guardar_SkillYaGuardadaPorJugador_TraduceConflictoASkillYaLanzada()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = _baseDeDatos.Contexto.Skills.Single(s => s.Id == 3);
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 3, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));

        Assert.ThrowsExactly<SkillYaLanzadaException>(_repositorio.Guardar);

        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
    }

    [TestMethod]
    public void Guardar_EstadoModificadoPorOtroPedido_TraduceConflictoABatallaModificada()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 3, batalla.Jugador1, skill, FechaLanzamiento.AddMinutes(2)));
        _baseDeDatos.Contexto.Entry(batalla).Property(b => b.JugadorTurnoId).CurrentValue = 8;
        using var otroContexto = CrearOtroContexto();
        var cancelada = otroContexto.Batallas.Single(b => b.Id == batalla.Id);
        cancelada.Cancelar();
        otroContexto.SaveChanges();

        Assert.ThrowsExactly<BatallaModificadaException>(_repositorio.Guardar);

        var guardada = _baseDeDatos.Contexto.Batallas.Single(b => b.Id == batalla.Id);
        Assert.AreEqual(EstadoBatalla.Cancelada, guardada.Estado);
        Assert.IsNull(guardada.JugadorTurnoId);
        Assert.AreEqual(2, _baseDeDatos.Contexto.Lanzamientos.Count(l => l.BatallaId == batalla.Id));
    }

    [TestMethod]
    public void ObtenerPorId_TorneoConObligatoria_CargaLaSkillAunqueSeHayaDesactivado()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = _baseDeDatos.Contexto.Skills.Single(s => s.Id == 3);
        batalla.Torneo.AsociarSkillObligatoria(skill);
        skill.Desactivar();
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.ObtenerPorId(batalla.Id);

        Assert.IsNotNull(resultado);
        Assert.HasCount(1, resultado.Torneo.SkillsObligatorias);
        var obligatoria = resultado.Torneo.SkillsObligatorias.Single();
        Assert.AreEqual(3, obligatoria.Id);
        Assert.IsFalse(obligatoria.Activa);
    }

    [TestMethod]
    public void Guardar_ErrorDeRestriccionSinDuplicados_PropagaErrorDePersistenciaSinGuardar()
    {
        var batalla = GuardarBatallaConHistorial();
        var skill = new Skill("AURASTARE", "Aura Stare", "Mirada fija para farmear aura en la batalla.", CategoriaSkill.Gestual, 2) { Id = 4 };
        _baseDeDatos.Contexto.Lanzamientos.Add(new Lanzamiento(batalla, 9, batalla.Jugador1, skill, FechaLanzamiento));
        _baseDeDatos.Contexto.Entry(batalla).Property(b => b.JugadorTurnoId).CurrentValue = 8;

        Assert.ThrowsExactly<DbUpdateException>(_repositorio.Guardar);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.HasCount(2, _baseDeDatos.Contexto.Lanzamientos.ToList());
        Assert.AreEqual(7, _baseDeDatos.Contexto.Batallas.Single(b => b.Id == batalla.Id).JugadorTurnoId);
        Assert.IsFalse(_baseDeDatos.Contexto.Skills.Any(s => s.Id == 4));
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Pendiente, 7, true)]
    [DataRow(EstadoBatalla.EnArbitraje, 8, true)]
    [DataRow(EstadoBatalla.Finalizada, 7, false)]
    [DataRow(EstadoBatalla.Cancelada, 8, false)]
    public void ExisteSinResolverDelJugador_BatallaEnUnEstado_IndicaSiEstaPendienteOEnArbitraje(EstadoBatalla estado, int jugadorId, bool esperado)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var otro1 = CrearUsuario(9, Rol.Jugador);
        var otro2 = CrearUsuario(10, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, CrearUsuario(2, Rol.Organizador), FechaActual)
        { Id = 5 };
        var propia = new Batalla(torneo, 1, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = 12 };
        _baseDeDatos.Contexto.Batallas.AddRange(propia, new Batalla(torneo, 1, otro1, otro2, new SorteoInicial(otro1)) { Id = 13 });

        // Los lanzamientos y el fallo no importan para la consulta: se fija el estado directamente.
        _baseDeDatos.Contexto.Entry(propia).Property(b => b.Estado).CurrentValue = estado;
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var existe = _repositorio.ExisteSinResolverDelJugador(jugadorId);

        Assert.AreEqual(esperado, existe);
    }

    private FarmeORTContext CrearOtroContexto()
    {
        var opciones = new DbContextOptionsBuilder<FarmeORTContext>()
            .UseSqlite(_baseDeDatos.Contexto.Database.GetDbConnection())
            .Options;
        return new FarmeORTContext(opciones);
    }

    private Batalla GuardarBatallaConHistorial()
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, CrearUsuario(2, Rol.Organizador), FechaActual)
        { Id = 5 };
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
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual)
        { Id = id };
    }

    private sealed class SorteoInicial(Usuario elegido) : ISorteo
    {
        public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
        {
            return elegido;
        }
    }
}
