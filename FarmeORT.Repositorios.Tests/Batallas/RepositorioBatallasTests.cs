using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
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
    [DataRow(null, null, null, 20, 0, "14,11,12,16,15")]
    [DataRow(EstadoBatalla.Pendiente, null, null, 20, 0, "14,12")]
    [DataRow(EstadoBatalla.Cancelada, null, null, 20, 0, "11")]
    [DataRow(EstadoBatalla.Finalizada, null, null, 20, 0, "15")]
    [DataRow(EstadoBatalla.EnArbitraje, null, null, 20, 0, "16")]
    [DataRow(null, 2, null, 20, 0, "11,12,16")]
    [DataRow(null, null, 7, 20, 0, "14,11,12,15")]
    [DataRow(null, null, 8, 20, 0, "14,11,12,15")]
    [DataRow(null, null, 9, 20, 0, "16")]
    [DataRow(EstadoBatalla.Pendiente, 2, 8, 20, 0, "12")]
    [DataRow(null, null, null, 2, 1, "11,12")]
    [DataRow(null, 99, null, 20, 0, "")]
    public void Listar_FiltrosYPagina_OrdenaPorRondaEIdEnLaConsulta(
        EstadoBatalla? estado, int? ronda, int? jugadorId, int limite, int desplazamiento, string idsEsperados)
    {
        var original = GuardarBatallaConHistorial();
        var torneo = original.Torneo;
        var jugador1 = original.Jugador1;
        var jugador2 = original.Jugador2;
        var primera = new Batalla(torneo, 1, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = 14 };
        var cancelada = new Batalla(torneo, 2, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = 11 };
        cancelada.Cancelar();
        var finalizada = new Batalla(torneo, 3, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = 15 };
        finalizada.Fallar(8, "Victoria por abandono.", -16, 16);
        var otroJugador = CrearUsuario(9, Rol.Jugador);
        var otros = new Batalla(torneo, 2, otroJugador, CrearUsuario(10, Rol.Jugador), new SorteoInicial(otroJugador)) { Id = 16 };
        var otroTorneo = new Torneo("COPAFUEGO", "Copa Fuego", "Otro torneo eliminatorio de batallas.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 4, torneo.Organizador, FechaActual)
        { Id = 6 };
        var ajena = new Batalla(otroTorneo, 1, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = 17 };
        _baseDeDatos.Contexto.Batallas.AddRange(primera, cancelada, finalizada, otros, ajena);
        _baseDeDatos.Contexto.Entry(otros).Property(b => b.Estado).CurrentValue = EstadoBatalla.EnArbitraje;
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroBatallas(5, estado, ronda, jugadorId, limite, desplazamiento));

        var esperados = idsEsperados.Length == 0 ? Array.Empty<int>() : idsEsperados.Split(',').Select(int.Parse).ToArray();
        CollectionAssert.AreEqual(esperados, resultado.Select(b => b.Id).ToArray());
        Assert.IsTrue(resultado.All(b => b.Jugador1.Nickname == $"usuario{b.Jugador1Id}" && b.Jugador2.Nickname == $"usuario{b.Jugador2Id}"));
        Assert.IsTrue(resultado.All(b => b.TorneoId == 5));
        Assert.AreEqual(0, _baseDeDatos.Contexto.ChangeTracker.Entries().Count());
    }

    [TestMethod]
    public void Guardar_Fallo_PersisteGanadorComentarioEloYEliminacionJuntos()
    {
        var batalla = GuardarBatallaConHistorial();
        var inscripcion = new Inscripcion(batalla.Torneo, batalla.Jugador2, FechaActual.ToDateTime(TimeOnly.MinValue));
        _baseDeDatos.Contexto.Inscripciones.Add(inscripcion);
        _baseDeDatos.Contexto.SaveChanges();
        batalla.Jugador1.ActualizarElo(1216);
        batalla.Jugador2.ActualizarElo(1184);
        batalla.Fallar(7, "Victoria por abandono.", 16, -16);
        inscripcion.Eliminar();

        _repositorio.Guardar();

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var guardada = _baseDeDatos.Contexto.Batallas.Include(b => b.Ganador).Single(b => b.Id == 12);
        Assert.AreEqual(EstadoBatalla.Finalizada, guardada.Estado);
        Assert.AreEqual(7, guardada.GanadorId);
        Assert.AreEqual(7, guardada.Ganador!.Id);
        Assert.AreEqual("Victoria por abandono.", guardada.Comentario);
        Assert.AreEqual(16, guardada.CambioEloJugador1);
        Assert.AreEqual(-16, guardada.CambioEloJugador2);
        Assert.IsNull(guardada.JugadorTurnoId);
        Assert.AreEqual(1216, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == 7).Elo);
        Assert.AreEqual(1184, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == 8).Elo);
        Assert.AreEqual(EstadoInscripcion.Eliminada, _baseDeDatos.Contexto.Inscripciones.Single(i => i.Id == inscripcion.Id).Estado);
    }

    [TestMethod]
    public void Guardar_CancelacionConcurrente_RevierteTodosLosEfectosDelFallo()
    {
        var batalla = GuardarBatallaConHistorial();
        var inscripcion = new Inscripcion(batalla.Torneo, batalla.Jugador2, FechaActual.ToDateTime(TimeOnly.MinValue));
        _baseDeDatos.Contexto.Inscripciones.Add(inscripcion);
        _baseDeDatos.Contexto.SaveChanges();
        batalla.Jugador1.ActualizarElo(1216);
        batalla.Jugador2.ActualizarElo(1184);
        batalla.Fallar(7, "Victoria por abandono.", 16, -16);
        inscripcion.Eliminar();
        using var otroContexto = CrearOtroContexto();
        var cancelada = otroContexto.Batallas.Single(b => b.Id == 12);
        cancelada.Cancelar();
        otroContexto.SaveChanges();

        Assert.ThrowsExactly<BatallaModificadaException>(_repositorio.Guardar);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var guardada = _baseDeDatos.Contexto.Batallas.Single(b => b.Id == 12);
        Assert.AreEqual(EstadoBatalla.Cancelada, guardada.Estado);
        Assert.IsNull(guardada.GanadorId);
        Assert.IsNull(guardada.Comentario);
        Assert.AreEqual(1200, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == 7).Elo);
        Assert.AreEqual(1200, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == 8).Elo);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _baseDeDatos.Contexto.Inscripciones.Single(i => i.Id == inscripcion.Id).Estado);
    }

    [TestMethod]
    public void Guardar_EloConcurrente_RechazaFalloSinGuardarEfectosParciales()
    {
        var batalla = GuardarBatallaConHistorial();
        var inscripcion = new Inscripcion(batalla.Torneo, batalla.Jugador2, FechaActual.ToDateTime(TimeOnly.MinValue));
        _baseDeDatos.Contexto.Inscripciones.Add(inscripcion);
        _baseDeDatos.Contexto.SaveChanges();
        batalla.Jugador1.ActualizarElo(1216);
        batalla.Jugador2.ActualizarElo(1184);
        batalla.Fallar(7, "Victoria por abandono.", 16, -16);
        inscripcion.Eliminar();
        using var otroContexto = CrearOtroContexto();
        var jugador = otroContexto.Usuarios.Single(u => u.Id == 7);
        jugador.ActualizarElo(1400);
        otroContexto.SaveChanges();

        Assert.ThrowsExactly<BatallaModificadaException>(_repositorio.Guardar);

        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(EstadoBatalla.Pendiente, _baseDeDatos.Contexto.Batallas.Single(b => b.Id == 12).Estado);
        Assert.IsNull(_baseDeDatos.Contexto.Batallas.Single(b => b.Id == 12).GanadorId);
        Assert.AreEqual(1400, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == 7).Elo);
        Assert.AreEqual(1200, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == 8).Elo);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _baseDeDatos.Contexto.Inscripciones.Single(i => i.Id == inscripcion.Id).Estado);
    }

    [TestMethod]
    public void Persistencia_Fallo_ConfiguraGanadorOpcionalYComentarioAcotado()
    {
        var entidad = _baseDeDatos.Contexto.Model.FindEntityType(typeof(Batalla))!;

        var ganador = entidad.FindNavigation(nameof(Batalla.Ganador))!;

        Assert.AreEqual(500, entidad.FindProperty(nameof(Batalla.Comentario))!.GetMaxLength());
        Assert.IsTrue(entidad.FindProperty(nameof(Batalla.GanadorId))!.IsNullable);
        Assert.AreEqual(nameof(Batalla.GanadorId), ganador.ForeignKey.Properties.Single().Name);
        Assert.AreEqual(DeleteBehavior.Restrict, ganador.ForeignKey.DeleteBehavior);
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
    public void ListarSinResolverDelTorneo_BatallasEnTodosLosEstados_DevuelveRastreadasLasPendientesYEnArbitrajeDelTorneo()
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var organizador = CrearUsuario(2, Rol.Organizador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, organizador, FechaActual)
        { Id = 5 };
        var otroTorneo = new Torneo("COPAFARM", "Copa Farm", "Otro torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, organizador, FechaActual)
        { Id = 6 };
        var sorteo = new SorteoInicial(jugador1);
        var enArbitraje = new Batalla(torneo, 1, jugador1, jugador2, sorteo) { Id = 13 };
        var cancelada = new Batalla(torneo, 1, jugador1, jugador2, sorteo) { Id = 14 };
        var finalizada = new Batalla(torneo, 1, jugador1, jugador2, sorteo) { Id = 15 };
        cancelada.Cancelar();
        _baseDeDatos.Contexto.Batallas.AddRange(
            new Batalla(torneo, 1, jugador1, jugador2, sorteo) { Id = 12 },
            enArbitraje,
            cancelada,
            finalizada,
            new Batalla(otroTorneo, 1, jugador1, jugador2, sorteo) { Id = 16 });

        // Los ocho lanzamientos y el fallo no importan para la consulta: se fija el estado directamente.
        _baseDeDatos.Contexto.Entry(enArbitraje).Property(b => b.Estado).CurrentValue = EstadoBatalla.EnArbitraje;
        _baseDeDatos.Contexto.Entry(finalizada).Property(b => b.Estado).CurrentValue = EstadoBatalla.Finalizada;
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.ListarSinResolverDelTorneo(5);

        CollectionAssert.AreEqual(new[] { 12, 13 }, resultado.Select(b => b.Id).ToArray());
        Assert.IsTrue(resultado.All(b => _baseDeDatos.Contexto.Entry(b).State == EntityState.Unchanged));
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

    [TestMethod]
    public void ContarResultadosPorJugador_BatallasFalladasYPendientes_CuentaGanadasYPerdidasSoloDeLasFinalizadas()
    {
        var torneo = CrearTorneoConId(5, "COPAAURA", CrearUsuario(2, Rol.Organizador));
        var jugador7 = CrearUsuario(7, Rol.Jugador);
        var jugador8 = CrearUsuario(8, Rol.Jugador);
        var jugador9 = CrearUsuario(9, Rol.Jugador);
        var jugador10 = CrearUsuario(10, Rol.Jugador);
        _baseDeDatos.Contexto.Batallas.AddRange(
            BatallaFallada(12, torneo, jugador7, jugador8, ganador: jugador7),
            BatallaFallada(13, torneo, jugador7, jugador9, ganador: jugador9),
            BatallaFallada(14, torneo, jugador8, jugador9, ganador: jugador8),
            BatallaFallada(15, torneo, jugador7, jugador10, ganador: jugador10),
            new Batalla(torneo, 1, jugador7, jugador8, new SorteoInicial(jugador7)) { Id = 16 });
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultados = _repositorio.ContarResultadosPorJugador([7, 8]);

        Assert.HasCount(2, resultados);
        Assert.AreEqual(new ResultadosDeBatallas(1, 2), resultados[7]);
        Assert.AreEqual(new ResultadosDeBatallas(1, 1), resultados[8]);
    }

    [TestMethod]
    public void ContarResultadosPorJugador_BatallasDeVariosTorneos_CuentaLasDeTodos()
    {
        var organizador = CrearUsuario(2, Rol.Organizador);
        var torneo = CrearTorneoConId(5, "COPAAURA", organizador);
        var otroTorneo = CrearTorneoConId(6, "COPAFARM", organizador);
        var jugador7 = CrearUsuario(7, Rol.Jugador);
        var jugador8 = CrearUsuario(8, Rol.Jugador);
        _baseDeDatos.Contexto.Batallas.AddRange(
            BatallaFallada(12, torneo, jugador7, jugador8, ganador: jugador7),
            BatallaFallada(13, otroTorneo, jugador7, jugador8, ganador: jugador8),
            BatallaFallada(14, otroTorneo, jugador8, jugador7, ganador: jugador7));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultados = _repositorio.ContarResultadosPorJugador([7]);

        Assert.HasCount(1, resultados);
        Assert.AreEqual(new ResultadosDeBatallas(2, 1), resultados[7]);
    }

    [TestMethod]
    public void ContarResultadosPorJugador_JugadorSinBatallasFinalizadas_NoFigura()
    {
        var torneo = CrearTorneoConId(5, "COPAAURA", CrearUsuario(2, Rol.Organizador));
        var jugador7 = CrearUsuario(7, Rol.Jugador);
        var jugador8 = CrearUsuario(8, Rol.Jugador);
        _baseDeDatos.Contexto.Batallas.Add(new Batalla(torneo, 1, jugador7, jugador8, new SorteoInicial(jugador7)) { Id = 12 });
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultados = _repositorio.ContarResultadosPorJugador([7, 8]);

        Assert.IsEmpty(resultados);
    }

    private static Torneo CrearTorneoConId(int id, string codigo, Usuario organizador)
    {
        return new Torneo(codigo, "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, organizador, FechaActual)
        { Id = id };
    }

    private static Batalla BatallaFallada(int id, Torneo torneo, Usuario jugador1, Usuario jugador2, Usuario ganador)
    {
        var batalla = new Batalla(torneo, 1, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = id };
        batalla.Fallar(ganador.Id, "Ganó por mejor ejecución de las skills.", 16, -16);
        return batalla;
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
