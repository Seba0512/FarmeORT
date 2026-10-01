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
