using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Batallas;

[TestClass]
public sealed class BatallaTests
{
    private const int BatallaId = 12;
    private const int IdJugador1 = 7;
    private const int IdJugador2 = 8;

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateTimeOffset Fecha = new(2026, 10, 1, 17, 0, 0, TimeSpan.Zero);

    [TestMethod]
    [DataRow(IdJugador1, 10)]
    [DataRow(IdJugador2, 500)]
    public void Fallar_ComentarioEnLimites_ConservaFalloYCambiosDeElo(int ganadorId, int largo)
    {
        var batalla = CrearBatallaConHistorial(0);
        var comentario = new string('a', largo);

        batalla.Fallar(ganadorId, comentario, 16, -16);

        Assert.AreEqual(EstadoBatalla.Finalizada, batalla.Estado);
        Assert.AreEqual(ganadorId, batalla.GanadorId);
        Assert.IsNotNull(batalla.Ganador);
        Assert.AreEqual(ganadorId, batalla.Ganador.Id);
        Assert.AreEqual(comentario, batalla.Comentario);
        Assert.AreEqual(16, batalla.CambioEloJugador1);
        Assert.AreEqual(-16, batalla.CambioEloJugador2);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("          ")]
    public void Fallar_ComentarioVacio_RechazaSinCambiarEstado(string? comentario)
    {
        var batalla = CrearBatallaConHistorial(0);

        Assert.ThrowsExactly<DatosInvalidosException>(() => batalla.Fallar(IdJugador1, comentario, 16, -16));

        AssertSinFallo(batalla);
    }

    [TestMethod]
    [DataRow(9)]
    [DataRow(501)]
    public void Fallar_ComentarioFueraDeRango_RechazaSinCambiarEstado(int largo)
    {
        var batalla = CrearBatallaConHistorial(0);
        var comentario = new string('a', largo);

        Assert.ThrowsExactly<DatosInvalidosException>(() => batalla.Fallar(IdJugador1, comentario, 16, -16));

        AssertSinFallo(batalla);
    }

    [TestMethod]
    public void Fallar_BatallaEnArbitraje_QuedaFinalizadaConElGanador()
    {
        var batalla = CrearBatallaConHistorial(8);

        batalla.Fallar(IdJugador2, "Farmeó más aura en los ocho turnos.", -16, 16);

        Assert.AreEqual(EstadoBatalla.Finalizada, batalla.Estado);
        Assert.AreEqual(IdJugador2, batalla.GanadorId);
        Assert.HasCount(8, batalla.Lanzamientos);
    }

    [TestMethod]
    public void Fallar_GanadorAjeno_RechazaSinRegistrarFallo()
    {
        var batalla = CrearBatallaConHistorial(0);

        Assert.ThrowsExactly<JugadorNoParticipaEnBatallaException>(() => batalla.Fallar(9, "Victoria por abandono.", 16, -16));

        Assert.IsNull(batalla.GanadorId);
        Assert.AreEqual(EstadoBatalla.Pendiente, batalla.Estado);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Fallar_BatallaResuelta_RechazaSinRegistrarFallo(EstadoBatalla estado)
    {
        var batalla = CrearBatallaEnEstado(estado);
        var ganadorPrevio = batalla.GanadorId;

        Assert.ThrowsExactly<BatallaNoFallableException>(() => batalla.Fallar(IdJugador2, "Otro fallo distinto.", -16, 16));

        Assert.AreEqual(ganadorPrevio, batalla.GanadorId);
        Assert.AreEqual(estado, batalla.Estado);
    }

    [TestMethod]
    public void Constructor_EloEmpatado_IniciaPendienteConJugadorElegidoPorSorteo()
    {
        var jugador1 = CrearUsuario(IdJugador1, Rol.Jugador);
        var jugador2 = CrearUsuario(IdJugador2, Rol.Jugador);
        var torneo = CrearTorneo();
        var sorteo = new SorteoPreparado(jugador2);

        var batalla = new Batalla(torneo, 2, jugador1, jugador2, sorteo);

        Assert.AreSame(torneo, batalla.Torneo);
        Assert.AreEqual(torneo.Id, batalla.TorneoId);
        Assert.AreEqual(2, batalla.NumeroRonda);
        Assert.AreSame(jugador1, batalla.Jugador1);
        Assert.AreEqual(IdJugador1, batalla.Jugador1Id);
        Assert.AreSame(jugador2, batalla.Jugador2);
        Assert.AreEqual(IdJugador2, batalla.Jugador2Id);
        Assert.AreEqual(EstadoBatalla.Pendiente, batalla.Estado);
        Assert.AreEqual(IdJugador2, batalla.JugadorInicialId);
        Assert.AreEqual(IdJugador2, batalla.JugadorTurnoId);
        Assert.AreEqual(1, sorteo.Llamadas);
        Assert.AreSame(jugador1, sorteo.Jugador1Recibido);
        Assert.AreSame(jugador2, sorteo.Jugador2Recibido);
    }

    [TestMethod]
    [DataRow(1300, 1200, IdJugador1)]
    [DataRow(1200, 1300, IdJugador2)]
    public void Constructor_EloDistinto_EmpiezaElMayorSinSortear(int elo1, int elo2, int esperado)
    {
        var jugador1 = CrearUsuario(IdJugador1, Rol.Jugador);
        var jugador2 = CrearUsuario(IdJugador2, Rol.Jugador);
        jugador1.ActualizarElo(elo1);
        jugador2.ActualizarElo(elo2);
        var sorteo = new SorteoPreparado(jugador1);

        var batalla = new Batalla(CrearTorneo(), 1, jugador1, jugador2, sorteo);

        Assert.AreEqual(esperado, batalla.JugadorInicialId);
        Assert.AreEqual(esperado, batalla.JugadorTurnoId);
        Assert.AreEqual(0, sorteo.Llamadas);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Pendiente)]
    [DataRow(EstadoBatalla.EnArbitraje)]
    public void Cancelar_BatallaSinResolver_PasaACanceladaYSinTurno(EstadoBatalla estado)
    {
        var batalla = CrearBatallaEnEstado(estado);

        batalla.Cancelar();

        Assert.AreEqual(EstadoBatalla.Cancelada, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Cancelar_BatallaResuelta_LanzaBatallaNoCancelableSinCambiarEstado(EstadoBatalla estado)
    {
        var batalla = CrearBatallaEnEstado(estado);

        Assert.ThrowsExactly<BatallaNoCancelableException>(batalla.Cancelar);

        Assert.AreEqual(estado, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(IdJugador1, IdJugador2)]
    [DataRow(IdJugador2, IdJugador1)]
    public void Lanzar_PrimerTurno_RegistraSkillYAlternaJugador(int inicialId, int siguienteId)
    {
        var batalla = CrearBatallaConHistorial(0, inicialId);
        var inicial = JugadorDelTurno(batalla);
        var skill = inicial.Repertorio.Single(d => d.SkillId == 3).Skill;

        var lanzamiento = batalla.Lanzar(inicial, skill, Fecha);

        Assert.HasCount(1, batalla.Lanzamientos);
        Assert.AreSame(lanzamiento, batalla.Lanzamientos.Single());
        Assert.AreSame(batalla, lanzamiento.Batalla);
        Assert.AreEqual(BatallaId, lanzamiento.BatallaId);
        Assert.AreEqual(1, lanzamiento.Turno);
        Assert.AreSame(inicial, lanzamiento.Jugador);
        Assert.AreEqual(inicialId, lanzamiento.JugadorId);
        Assert.AreSame(skill, lanzamiento.Skill);
        Assert.AreEqual(3, lanzamiento.SkillId);
        Assert.AreEqual(Fecha, lanzamiento.Fecha);
        Assert.AreEqual(EstadoBatalla.Pendiente, batalla.Estado);
        Assert.AreEqual(siguienteId, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(IdJugador1)]
    [DataRow(IdJugador2)]
    public void Lanzar_OctavoTurno_CompletaCuatroPorJugadorYPasaAEnArbitraje(int inicialId)
    {
        var batalla = CrearBatallaConHistorial(7, inicialId);
        var jugador = JugadorDelTurno(batalla);
        var skill = jugador.Repertorio.Single(d => d.SkillId == 4).Skill;

        var lanzamiento = batalla.Lanzar(jugador, skill, Fecha);

        Assert.AreEqual(8, lanzamiento.Turno);
        Assert.HasCount(8, batalla.Lanzamientos);
        Assert.AreEqual(4, batalla.Lanzamientos.Count(l => l.JugadorId == IdJugador1));
        Assert.AreEqual(4, batalla.Lanzamientos.Count(l => l.JugadorId == IdJugador2));
        Assert.AreEqual(EstadoBatalla.EnArbitraje, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.EnArbitraje)]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Lanzar_BatallaNoPendiente_RechazaSinModificarHistorial(EstadoBatalla estado)
    {
        var batalla = CrearBatallaEnEstado(estado);
        var lanzamientosPrevios = batalla.Lanzamientos.Count;
        var skill = batalla.Jugador1.Repertorio.First().Skill;

        Assert.ThrowsExactly<BatallaNoPendienteException>(() => batalla.Lanzar(batalla.Jugador1, skill, Fecha));

        Assert.HasCount(lanzamientosPrevios, batalla.Lanzamientos);
        Assert.AreEqual(estado, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Lanzar_JugadorAjeno_RechazaSinConsumirTurno()
    {
        var batalla = CrearBatallaConHistorial(0);
        var jugador = CrearUsuario(9, Rol.Jugador);
        var skill = CrearSkill(1);
        jugador.DeclararSkill(skill, 4, FechaActual);

        Assert.ThrowsExactly<JugadorNoParticipaEnBatallaException>(() => batalla.Lanzar(jugador, skill, Fecha));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(IdJugador1, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(IdJugador1)]
    [DataRow(IdJugador2)]
    public void Lanzar_TurnoDelRival_RechazaSinConsumirTurno(int inicialId)
    {
        var batalla = CrearBatallaConHistorial(0, inicialId);
        var rival = JugadorSinTurno(batalla);
        var skill = rival.Repertorio.First().Skill;

        Assert.ThrowsExactly<TurnoIncorrectoException>(() => batalla.Lanzar(rival, skill, Fecha));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(inicialId, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Lanzar_SkillNoDeclarada_RechazaSinConsumirTurno()
    {
        var batalla = CrearBatallaConHistorial(0);
        var skill = CrearSkill(99);

        Assert.ThrowsExactly<SkillFueraDelRepertorioException>(() => batalla.Lanzar(batalla.Jugador1, skill, Fecha));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(IdJugador1, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(IdJugador1)]
    [DataRow(IdJugador2)]
    public void Lanzar_SkillYaUsadaPorElJugador_RechazaSinConsumirTurno(int inicialId)
    {
        var batalla = CrearBatallaConHistorial(2, inicialId);
        var jugador = JugadorDelTurno(batalla);
        var skill = jugador.Repertorio.Single(d => d.SkillId == 1).Skill;

        Assert.ThrowsExactly<SkillYaLanzadaException>(() => batalla.Lanzar(jugador, skill, Fecha));

        Assert.HasCount(2, batalla.Lanzamientos);
        Assert.AreEqual(inicialId, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Lanzar_SkillDesactivadaDespuesDeDeclararla_RegistraElLanzamiento()
    {
        var batalla = CrearBatallaConHistorial(0);
        var skill = batalla.Jugador1.Repertorio.Single(d => d.SkillId == 1).Skill;
        skill.Desactivar();

        var lanzamiento = batalla.Lanzar(batalla.Jugador1, skill, Fecha);

        Assert.AreEqual(1, lanzamiento.SkillId);
        Assert.IsFalse(lanzamiento.Skill.Activa);
        Assert.HasCount(1, batalla.Lanzamientos);
    }

    [TestMethod]
    public void Lanzar_SkillUsadaSoloPorElRival_PermiteLanzarla()
    {
        var batalla = CrearBatallaConHistorial(1);
        var skill = batalla.Jugador2.Repertorio.Single(d => d.SkillId == 1).Skill;

        var lanzamiento = batalla.Lanzar(batalla.Jugador2, skill, Fecha);

        Assert.AreEqual(2, lanzamiento.Turno);
        Assert.HasCount(2, batalla.Lanzamientos);
        Assert.IsTrue(batalla.Lanzamientos.All(l => l.SkillId == 1));
        Assert.AreEqual(IdJugador1, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Constructor_MismoJugadorEnAmbosLados_RechazaDatosInvalidos()
    {
        var jugador1 = CrearUsuario(IdJugador1, Rol.Jugador);
        var jugador2 = CrearUsuario(IdJugador1, Rol.Jugador);

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => new Batalla(CrearTorneo(), 1, jugador1, jugador2, new SorteoPreparado(jugador1)));
    }

    [TestMethod]
    [DataRow(Rol.Organizador, Rol.Jugador)]
    [DataRow(Rol.Administrador, Rol.Jugador)]
    [DataRow(Rol.Jugador, Rol.Organizador)]
    [DataRow(Rol.Jugador, Rol.Administrador)]
    public void Constructor_ParticipanteSinRolJugador_RechazaDatosInvalidos(Rol rol1, Rol rol2)
    {
        var jugador1 = CrearUsuario(IdJugador1, rol1);
        var jugador2 = CrearUsuario(IdJugador2, rol2);

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => new Batalla(CrearTorneo(), 1, jugador1, jugador2, new SorteoPreparado(jugador1)));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Constructor_RondaNoPositiva_RechazaDatosInvalidos(int ronda)
    {
        var jugador1 = CrearUsuario(IdJugador1, Rol.Jugador);
        var jugador2 = CrearUsuario(IdJugador2, Rol.Jugador);

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => new Batalla(CrearTorneo(), ronda, jugador1, jugador2, new SorteoPreparado(jugador1)));
    }

    [TestMethod]
    [DataRow(4, IdJugador1)]
    [DataRow(4, IdJugador2)]
    [DataRow(5, IdJugador1)]
    [DataRow(5, IdJugador2)]
    [DataRow(6, IdJugador1)]
    [DataRow(6, IdJugador2)]
    public void Lanzar_ObligatoriasPendientesIgualATurnosPropios_RechazaLibre(int lanzamientos, int inicialId)
    {
        var batalla = CrearBatallaConHistorial(lanzamientos, inicialId);
        var jugador = JugadorDelTurno(batalla);
        var libre = CrearSkill(99);
        jugador.DeclararSkill(libre, 4, FechaActual);
        AsociarObligatorias(batalla);

        Assert.ThrowsExactly<SkillObligatoriaPendienteException>(() => batalla.Lanzar(jugador, libre, Fecha));

        Assert.HasCount(lanzamientos, batalla.Lanzamientos);
        Assert.AreEqual(jugador.Id, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(0, IdJugador1)]
    [DataRow(0, IdJugador2)]
    [DataRow(2, IdJugador1)]
    [DataRow(2, IdJugador2)]
    public void Lanzar_HayMasTurnosPropiosQueObligatoriasPendientes_PermiteLibre(int lanzamientos, int inicialId)
    {
        var batalla = CrearBatallaConHistorial(lanzamientos, inicialId);
        var jugador = JugadorDelTurno(batalla);
        var libre = CrearSkill(99);
        jugador.DeclararSkill(libre, 4, FechaActual);
        AsociarObligatorias(batalla);

        var lanzamiento = batalla.Lanzar(jugador, libre, Fecha);

        Assert.AreEqual(99, lanzamiento.SkillId);
        Assert.AreEqual(lanzamientos + 1, lanzamiento.Turno);
    }

    [TestMethod]
    [DataRow(4, IdJugador1, 3)]
    [DataRow(4, IdJugador2, 4)]
    [DataRow(6, IdJugador1, 4)]
    [DataRow(6, IdJugador2, 4)]
    public void Lanzar_UltimosTurnosPropios_PermiteObligatoriaPendiente(int lanzamientos, int inicialId, int skillId)
    {
        var batalla = CrearBatallaConHistorial(lanzamientos, inicialId);
        var jugador = JugadorDelTurno(batalla);
        AsociarObligatorias(batalla);
        var obligatoria = jugador.Repertorio.Single(d => d.SkillId == skillId).Skill;
        obligatoria.Desactivar();

        var lanzamiento = batalla.Lanzar(jugador, obligatoria, Fecha);

        Assert.AreEqual(skillId, lanzamiento.SkillId);
        Assert.AreEqual(lanzamientos + 1, lanzamiento.Turno);
        Assert.IsFalse(lanzamiento.Skill.Activa);
    }

    [TestMethod]
    public void Lanzar_ObligatoriasYaCumplidas_PermiteLibreEnUltimoTurnoPropio()
    {
        var batalla = CrearBatallaConHistorial(6);
        var usada = batalla.Jugador1.Repertorio.Single(d => d.SkillId == 1).Skill;
        batalla.Torneo.AsociarSkillObligatoria(usada);
        var libre = CrearSkill(99);
        batalla.Jugador1.DeclararSkill(libre, 4, FechaActual);

        var lanzamiento = batalla.Lanzar(batalla.Jugador1, libre, Fecha);

        Assert.AreEqual(99, lanzamiento.SkillId);
        Assert.AreEqual(7, lanzamiento.Turno);
    }

    private static void AssertSinFallo(Batalla batalla)
    {
        Assert.AreEqual(EstadoBatalla.Pendiente, batalla.Estado);
        Assert.IsNull(batalla.GanadorId);
        Assert.IsNull(batalla.Comentario);
        Assert.IsNull(batalla.CambioEloJugador1);
        Assert.IsNull(batalla.CambioEloJugador2);
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual)
        { Id = id };
    }

    // Las skills 3 y 4 pasan a ser las obligatorias del torneo.
    private static void AsociarObligatorias(Batalla batalla)
    {
        foreach(var id in new[] { 3, 4 })
        {
            batalla.Torneo.AsociarSkillObligatoria(batalla.Jugador1.Repertorio.Single(d => d.SkillId == id).Skill);
        }
    }

    // Los dos jugadores declaran las skills 1 a 4 y las lanzan en orden, alternando desde el inicial: en los
    // turnos 1 y 2 cada uno lanza la skill 1, en los turnos 3 y 4 la skill 2, y así. Con 8 queda EnArbitraje.
    private static Batalla CrearBatallaConHistorial(int lanzamientos, int inicialId = IdJugador1)
    {
        var jugador1 = CrearUsuario(IdJugador1, Rol.Jugador);
        var jugador2 = CrearUsuario(IdJugador2, Rol.Jugador);
        var inicial = jugador1;
        if(inicialId == IdJugador2)
        {
            inicial = jugador2;
        }

        var batalla = new Batalla(CrearTorneo(), 1, jugador1, jugador2, new SorteoPreparado(inicial)) { Id = BatallaId };
        for(var id = 1; id <= 4; id++)
        {
            var skill = CrearSkill(id);
            jugador1.DeclararSkill(skill, 4, FechaActual);
            jugador2.DeclararSkill(skill, 4, FechaActual);
        }

        for(var turno = 1; turno <= lanzamientos; turno++)
        {
            var jugador = JugadorDelTurno(batalla);
            var skillId = (turno + 1) / 2;
            var skill = jugador.Repertorio.Single(d => d.SkillId == skillId).Skill;
            batalla.Lanzar(jugador, skill, new DateTimeOffset(2026, 10, 1, 16, turno, 0, TimeSpan.Zero));
        }

        return batalla;
    }

    // Lleva la batalla a cada estado con sus propias operaciones, sin tocar campos privados.
    private static Batalla CrearBatallaEnEstado(EstadoBatalla estado)
    {
        if(estado == EstadoBatalla.EnArbitraje)
        {
            return CrearBatallaConHistorial(8);
        }

        var batalla = CrearBatallaConHistorial(0);
        if(estado == EstadoBatalla.Finalizada)
        {
            batalla.Fallar(IdJugador1, "Victoria por abandono.", 16, -16);
        }

        if(estado == EstadoBatalla.Cancelada)
        {
            batalla.Cancelar();
        }

        return batalla;
    }

    private static Usuario JugadorDelTurno(Batalla batalla)
    {
        if(batalla.JugadorTurnoId == batalla.Jugador1Id)
        {
            return batalla.Jugador1;
        }

        return batalla.Jugador2;
    }

    private static Usuario JugadorSinTurno(Batalla batalla)
    {
        if(batalla.JugadorTurnoId == batalla.Jugador1Id)
        {
            return batalla.Jugador2;
        }

        return batalla.Jugador1;
    }

    private static Skill CrearSkill(int id)
    {
        return new Skill($"SKILL{id}", $"Skill {id}", "Skill para disputar una batalla de aura.", CategoriaSkill.Gestual, 2) { Id = id };
    }

    private static Torneo CrearTorneo()
    {
        return new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, CrearUsuario(2, Rol.Organizador), FechaActual)
        { Id = 5 };
    }

    private sealed class SorteoPreparado(Usuario elegido) : ISorteo
    {
        public int Llamadas { get; private set; }

        public Usuario? Jugador1Recibido { get; private set; }

        public Usuario? Jugador2Recibido { get; private set; }

        public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
        {
            Llamadas++;
            Jugador1Recibido = jugador1;
            Jugador2Recibido = jugador2;
            return elegido;
        }
    }
}
