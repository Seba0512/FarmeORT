using System.Reflection;
using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Batallas;

[TestClass]
public sealed class BatallaTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    [TestMethod]
    public void Constructor_EloEmpatado_IniciaPendienteConJugadorElegidoPorSorteo()
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = CrearTorneo();
        var sorteo = new SorteoPreparado(jugador2);

        var batalla = new Batalla(torneo, 2, jugador1, jugador2, sorteo);

        Assert.AreSame(torneo, batalla.Torneo);
        Assert.AreEqual(torneo.Id, batalla.TorneoId);
        Assert.AreEqual(2, batalla.NumeroRonda);
        Assert.AreSame(jugador1, batalla.Jugador1);
        Assert.AreEqual(7, batalla.Jugador1Id);
        Assert.AreSame(jugador2, batalla.Jugador2);
        Assert.AreEqual(8, batalla.Jugador2Id);
        Assert.AreEqual(EstadoBatalla.Pendiente, batalla.Estado);
        Assert.AreEqual(8, batalla.JugadorInicialId);
        Assert.AreEqual(8, batalla.JugadorTurnoId);
        Assert.AreEqual(1, sorteo.Llamadas);
        Assert.AreSame(jugador1, sorteo.Jugador1Recibido);
        Assert.AreSame(jugador2, sorteo.Jugador2Recibido);
    }

    [TestMethod]
    [DataRow(1300, 1200, 7)]
    [DataRow(1200, 1300, 8)]
    public void Constructor_EloDistinto_EmpiezaElMayorSinSortear(int elo1, int elo2, int esperado)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        typeof(Usuario).GetProperty(nameof(Usuario.Elo))!.SetValue(jugador1, elo1);
        typeof(Usuario).GetProperty(nameof(Usuario.Elo))!.SetValue(jugador2, elo2);
        var sorteo = new SorteoPreparado(esperado == 7 ? jugador2 : jugador1);

        var batalla = new Batalla(CrearTorneo(), 1, jugador1, jugador2, sorteo);

        Assert.AreEqual(esperado, batalla.JugadorInicialId);
        Assert.AreEqual(esperado, batalla.JugadorTurnoId);
        Assert.AreEqual(0, sorteo.Llamadas);
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual)
        { Id = id };
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Pendiente)]
    [DataRow(EstadoBatalla.EnArbitraje)]
    public void Cancelar_BatallaSinResolver_PasaACanceladaYSinTurno(EstadoBatalla estado)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var batalla = new Batalla(CrearTorneo(), 1, jugador1, CrearUsuario(8, Rol.Jugador), new SorteoPreparado(jugador1));
        typeof(Batalla).GetProperty(nameof(Batalla.Estado))!.SetValue(batalla, estado);

        batalla.Cancelar();

        Assert.AreEqual(EstadoBatalla.Cancelada, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Cancelar_BatallaResuelta_LanzaBatallaNoCancelableSinCambiarEstado(EstadoBatalla estado)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var batalla = new Batalla(CrearTorneo(), 1, jugador1, CrearUsuario(8, Rol.Jugador), new SorteoPreparado(jugador1));
        typeof(Batalla).GetProperty(nameof(Batalla.Estado))!.SetValue(batalla, estado);
        typeof(Batalla).GetProperty(nameof(Batalla.JugadorTurnoId))!.SetValue(batalla, null);

        Assert.ThrowsExactly<BatallaNoCancelableException>(() => batalla.Cancelar());

        Assert.AreEqual(estado, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(7, 8)]
    [DataRow(8, 7)]
    public void Lanzar_PrimerTurno_RegistraSkillYAlternaJugador(int inicialId, int siguienteId)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var inicial = inicialId == 7 ? jugador1 : jugador2;
        var batalla = new Batalla(CrearTorneo(), 1, jugador1, jugador2, new SorteoPreparado(inicial)) { Id = 12 };
        var skill = CrearSkill(3);
        inicial.DeclararSkill(skill, 4, FechaActual);
        var fecha = new DateTimeOffset(2026, 10, 1, 16, 30, 0, TimeSpan.Zero);

        var lanzamiento = batalla.Lanzar(inicial, skill, fecha);

        Assert.AreEqual(1, batalla.Lanzamientos.Count);
        Assert.AreSame(lanzamiento, batalla.Lanzamientos.Single());
        Assert.AreSame(batalla, lanzamiento.Batalla);
        Assert.AreEqual(12, lanzamiento.BatallaId);
        Assert.AreEqual(1, lanzamiento.Turno);
        Assert.AreSame(inicial, lanzamiento.Jugador);
        Assert.AreEqual(inicialId, lanzamiento.JugadorId);
        Assert.AreSame(skill, lanzamiento.Skill);
        Assert.AreEqual(3, lanzamiento.SkillId);
        Assert.AreEqual(fecha, lanzamiento.Fecha);
        Assert.AreEqual(EstadoBatalla.Pendiente, batalla.Estado);
        Assert.AreEqual(siguienteId, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(7)]
    [DataRow(8)]
    public void Lanzar_OctavoTurno_CompletaCuatroPorJugadorYPasaAEnArbitraje(int inicialId)
    {
        var batalla = CrearBatallaConHistorial(7, inicialId);
        var jugador = batalla.JugadorTurnoId == 7 ? batalla.Jugador1 : batalla.Jugador2;
        var skill = jugador.Repertorio.Single(d => d.SkillId == 4).Skill;
        var fecha = new DateTimeOffset(2026, 10, 1, 17, 0, 0, TimeSpan.Zero);

        var lanzamiento = batalla.Lanzar(jugador, skill, fecha);

        Assert.AreEqual(8, lanzamiento.Turno);
        Assert.HasCount(8, batalla.Lanzamientos);
        Assert.AreEqual(4, batalla.Lanzamientos.Count(l => l.JugadorId == 7));
        Assert.AreEqual(4, batalla.Lanzamientos.Count(l => l.JugadorId == 8));
        Assert.AreEqual(EstadoBatalla.EnArbitraje, batalla.Estado);
        Assert.IsNull(batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.EnArbitraje)]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Lanzar_BatallaNoPendiente_RechazaSinModificarHistorial(EstadoBatalla estado)
    {
        var batalla = CrearBatallaConHistorial(0);
        typeof(Batalla).GetProperty(nameof(Batalla.Estado))!.SetValue(batalla, estado);
        var skill = batalla.Jugador1.Repertorio.First().Skill;

        Assert.ThrowsExactly<BatallaNoPendienteException>(
            () => batalla.Lanzar(batalla.Jugador1, skill, DateTimeOffset.UtcNow));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(estado, batalla.Estado);
        Assert.AreEqual(7, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Lanzar_JugadorAjeno_RechazaSinConsumirTurno()
    {
        var batalla = CrearBatallaConHistorial(0);
        var jugador = CrearUsuario(9, Rol.Jugador);
        var skill = CrearSkill(1);
        jugador.DeclararSkill(skill, 4, FechaActual);

        Assert.ThrowsExactly<JugadorNoParticipaEnBatallaException>(
            () => batalla.Lanzar(jugador, skill, DateTimeOffset.UtcNow));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(7, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(7)]
    [DataRow(8)]
    public void Lanzar_TurnoDelRival_RechazaSinConsumirTurno(int inicialId)
    {
        var batalla = CrearBatallaConHistorial(0, inicialId);
        var rival = inicialId == 7 ? batalla.Jugador2 : batalla.Jugador1;
        var skill = rival.Repertorio.First().Skill;

        Assert.ThrowsExactly<TurnoIncorrectoException>(
            () => batalla.Lanzar(rival, skill, DateTimeOffset.UtcNow));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(inicialId, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Lanzar_SkillNoDeclarada_RechazaSinConsumirTurno()
    {
        var batalla = CrearBatallaConHistorial(0);
        var skill = CrearSkill(99);

        Assert.ThrowsExactly<SkillFueraDelRepertorioException>(
            () => batalla.Lanzar(batalla.Jugador1, skill, DateTimeOffset.UtcNow));

        Assert.HasCount(0, batalla.Lanzamientos);
        Assert.AreEqual(7, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(7)]
    [DataRow(8)]
    public void Lanzar_SkillYaUsadaPorElJugador_RechazaSinConsumirTurno(int inicialId)
    {
        var batalla = CrearBatallaConHistorial(2, inicialId);
        var jugador = inicialId == 7 ? batalla.Jugador1 : batalla.Jugador2;
        var skill = jugador.Repertorio.Single(d => d.SkillId == 1).Skill;

        Assert.ThrowsExactly<SkillYaLanzadaException>(
            () => batalla.Lanzar(jugador, skill, DateTimeOffset.UtcNow));

        Assert.HasCount(2, batalla.Lanzamientos);
        Assert.AreEqual(inicialId, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Lanzar_SkillDesactivadaDespuesDeDeclararla_RegistraElLanzamiento()
    {
        var batalla = CrearBatallaConHistorial(0);
        var skill = batalla.Jugador1.Repertorio.Single(d => d.SkillId == 1).Skill;
        skill.Desactivar();

        var lanzamiento = batalla.Lanzar(batalla.Jugador1, skill, DateTimeOffset.UtcNow);

        Assert.AreEqual(1, lanzamiento.SkillId);
        Assert.IsFalse(lanzamiento.Skill.Activa);
        Assert.HasCount(1, batalla.Lanzamientos);
    }

    [TestMethod]
    public void Lanzar_SkillUsadaSoloPorElRival_PermiteLanzarla()
    {
        var batalla = CrearBatallaConHistorial(1);
        var skill = batalla.Jugador2.Repertorio.Single(d => d.SkillId == 1).Skill;

        var lanzamiento = batalla.Lanzar(batalla.Jugador2, skill, DateTimeOffset.UtcNow);

        Assert.AreEqual(2, lanzamiento.Turno);
        Assert.HasCount(2, batalla.Lanzamientos);
        Assert.IsTrue(batalla.Lanzamientos.All(l => l.SkillId == 1));
        Assert.AreEqual(7, batalla.JugadorTurnoId);
    }

    [TestMethod]
    public void Constructor_MismoJugadorEnAmbosLados_RechazaDatosInvalidos()
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(7, Rol.Jugador);

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
        var jugador1 = CrearUsuario(7, rol1);
        var jugador2 = CrearUsuario(8, rol2);

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => new Batalla(CrearTorneo(), 1, jugador1, jugador2, new SorteoPreparado(jugador1)));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Constructor_RondaNoPositiva_RechazaDatosInvalidos(int ronda)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => new Batalla(CrearTorneo(), ronda, jugador1, CrearUsuario(8, Rol.Jugador), new SorteoPreparado(jugador1)));
    }

    [TestMethod]
    [DataRow(4, 7)]
    [DataRow(4, 8)]
    [DataRow(5, 7)]
    [DataRow(5, 8)]
    [DataRow(6, 7)]
    [DataRow(6, 8)]
    public void Lanzar_ObligatoriasPendientesIgualATurnosPropios_RechazaLibre(int cantidad, int inicialId)
    {
        var batalla = CrearBatallaConHistorial(cantidad, inicialId);
        var jugador = batalla.JugadorTurnoId == 7 ? batalla.Jugador1 : batalla.Jugador2;
        var libre = CrearSkill(99);
        jugador.DeclararSkill(libre, 4, FechaActual);
        AsociarObligatorias(batalla);

        Assert.ThrowsExactly<SkillObligatoriaPendienteException>(
            () => batalla.Lanzar(jugador, libre, DateTimeOffset.UtcNow));

        Assert.HasCount(cantidad, batalla.Lanzamientos);
        Assert.AreEqual(jugador.Id, batalla.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(0, 7)]
    [DataRow(0, 8)]
    [DataRow(2, 7)]
    [DataRow(2, 8)]
    public void Lanzar_HayMasTurnosPropiosQueObligatoriasPendientes_PermiteLibre(int cantidad, int inicialId)
    {
        var batalla = CrearBatallaConHistorial(cantidad, inicialId);
        var jugador = batalla.JugadorTurnoId == 7 ? batalla.Jugador1 : batalla.Jugador2;
        var libre = CrearSkill(99);
        jugador.DeclararSkill(libre, 4, FechaActual);
        AsociarObligatorias(batalla);

        var lanzamiento = batalla.Lanzar(jugador, libre, DateTimeOffset.UtcNow);

        Assert.AreEqual(99, lanzamiento.SkillId);
        Assert.AreEqual(cantidad + 1, lanzamiento.Turno);
    }

    [TestMethod]
    [DataRow(4, 7, 3)]
    [DataRow(4, 8, 4)]
    [DataRow(6, 7, 4)]
    [DataRow(6, 8, 4)]
    public void Lanzar_UltimosTurnosPropios_PermiteObligatoriaPendiente(int cantidad, int inicialId, int skillId)
    {
        var batalla = CrearBatallaConHistorial(cantidad, inicialId);
        var jugador = batalla.JugadorTurnoId == 7 ? batalla.Jugador1 : batalla.Jugador2;
        AsociarObligatorias(batalla);
        var obligatoria = jugador.Repertorio.Single(d => d.SkillId == skillId).Skill;
        obligatoria.Desactivar();

        var lanzamiento = batalla.Lanzar(jugador, obligatoria, DateTimeOffset.UtcNow);

        Assert.AreEqual(skillId, lanzamiento.SkillId);
        Assert.AreEqual(cantidad + 1, lanzamiento.Turno);
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

        var lanzamiento = batalla.Lanzar(batalla.Jugador1, libre, DateTimeOffset.UtcNow);

        Assert.AreEqual(99, lanzamiento.SkillId);
        Assert.AreEqual(7, lanzamiento.Turno);
    }

    private static void AsociarObligatorias(Batalla batalla)
    {
        foreach(var id in new[] { 3, 4 })
        {
            batalla.Torneo.AsociarSkillObligatoria(batalla.Jugador1.Repertorio.Single(d => d.SkillId == id).Skill);
        }
    }

    private static Batalla CrearBatallaConHistorial(int cantidad, int inicialId = 7)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var inicial = inicialId == 7 ? jugador1 : jugador2;
        var rival = inicialId == 7 ? jugador2 : jugador1;
        var batalla = new Batalla(CrearTorneo(), 1, jugador1, jugador2, new SorteoPreparado(inicial)) { Id = 12 };
        for(var id = 1; id <= 4; id++)
        {
            var skill = CrearSkill(id);
            jugador1.DeclararSkill(skill, 4, FechaActual);
            jugador2.DeclararSkill(skill, 4, FechaActual);
        }

        var historial = (List<Lanzamiento>)typeof(Batalla)
            .GetField("_lanzamientos", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(batalla)!;
        for(var turno = 1; turno <= cantidad; turno++)
        {
            var jugador = turno % 2 == 1 ? inicial : rival;
            var skill = jugador.Repertorio.Single(d => d.SkillId == ((turno + 1) / 2)).Skill;
            historial.Add(new Lanzamiento(batalla, turno, jugador, skill,
                new DateTimeOffset(2026, 10, 1, 16, turno, 0, TimeSpan.Zero)));
        }

        typeof(Batalla).GetProperty(nameof(Batalla.JugadorTurnoId))!.SetValue(batalla,
            cantidad % 2 == 0 ? inicial.Id : rival.Id);
        return batalla;
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
