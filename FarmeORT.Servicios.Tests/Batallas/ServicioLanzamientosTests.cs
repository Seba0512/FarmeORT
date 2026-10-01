using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Batallas;
using Moq;

namespace FarmeORT.Servicios.Tests.Batallas;

[TestClass]
public sealed class ServicioLanzamientosTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateTimeOffset FechaLanzamiento = new(2026, 10, 1, 16, 30, 0, TimeSpan.Zero);

    private Mock<IRepositorioBatallas> _batallas = null!;
    private Mock<IRepositorioUsuarios> _usuarios = null!;
    private Mock<IRepositorioSkills> _skills = null!;
    private Mock<TimeProvider> _reloj = null!;
    private ServicioLanzamientos _servicio = null!;
    private Batalla _batalla = null!;
    private Skill _skill = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _batallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _usuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _skills = new Mock<IRepositorioSkills>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _servicio = new ServicioLanzamientos(_batallas.Object, _usuarios.Object, _skills.Object, _reloj.Object);
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 8, CrearUsuario(2, Rol.Organizador), FechaActual) { Id = 5 };
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugador1, jugador2)).Returns(jugador1);
        _batalla = new Batalla(torneo, 1, jugador1, jugador2, sorteo.Object) { Id = 12 };
        _skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2) { Id = 3 };
        jugador1.DeclararSkill(_skill, 4, FechaActual);
    }

    [TestMethod]
    public void Lanzar_BatallaInexistente_LanzaBatallaNoEncontrada()
    {
        _batallas.Setup(r => r.ObtenerPorId(99)).Returns((Batalla?)null);

        Assert.ThrowsExactly<BatallaNoEncontradaException>(
            () => _servicio.Lanzar(99, 7, new LanzamientoRequest { SkillId = 3 }));

        _batallas.Verify(r => r.Guardar(), Times.Never);
    }

    [TestMethod]
    public void Lanzar_DatosValidos_GuardaUnaVezYDevuelveTurnoEstadoYSiguienteJugador()
    {
        ConfigurarLanzamientoValido();

        var resultado = _servicio.Lanzar(12, 7, new LanzamientoRequest { SkillId = 3 });

        _batallas.Verify(r => r.Guardar(), Times.Once);
        Assert.AreEqual(1, resultado.Turno);
        Assert.AreEqual("Pendiente", resultado.Estado);
        Assert.AreEqual(8, resultado.JugadorTurnoId);
        Assert.HasCount(1, _batalla.Lanzamientos);
        var lanzamiento = _batalla.Lanzamientos.Single();
        Assert.AreEqual(7, lanzamiento.JugadorId);
        Assert.AreEqual(3, lanzamiento.SkillId);
        Assert.AreEqual(FechaLanzamiento, lanzamiento.Fecha);
    }

    [TestMethod]
    public void Lanzar_JugadorInexistente_LanzaUsuarioNoEncontradoSinGuardar()
    {
        ConfigurarLanzamientoValido();
        _usuarios.Setup(r => r.ObtenerConRepertorio(7)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(
            () => _servicio.Lanzar(12, 7, new LanzamientoRequest { SkillId = 3 }));

        _batallas.Verify(r => r.Guardar(), Times.Never);
        _skills.Verify(r => r.ObtenerPorId(3), Times.Never);
    }

    [TestMethod]
    public void Lanzar_SkillInexistente_LanzaSkillNoEncontradaSinGuardar()
    {
        ConfigurarLanzamientoValido();
        _skills.Setup(r => r.ObtenerPorId(3)).Returns((Skill?)null);

        Assert.ThrowsExactly<SkillNoEncontradaException>(
            () => _servicio.Lanzar(12, 7, new LanzamientoRequest { SkillId = 3 }));

        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.HasCount(0, _batalla.Lanzamientos);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(-1)]
    public void Lanzar_SkillIdAusenteONoPositivo_RechazaDatosSinConsultarRepositorios(int? skillId)
    {
        ConfigurarLanzamientoValido();
        if(skillId != null)
        {
            _skills.Setup(r => r.ObtenerPorId(skillId.Value)).Returns(_skill);
        }

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => _servicio.Lanzar(12, 7, new LanzamientoRequest { SkillId = skillId }));

        _batallas.Verify(r => r.ObtenerPorId(It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
    }

    [TestMethod]
    public void Lanzar_DosObligatoriasPendientesYDosTurnosRestantes_RechazaSkillLibreSinGuardar()
    {
        ConfigurarLanzamientoValido();
        var historial = (List<Lanzamiento>)typeof(Batalla)
            .GetField("_lanzamientos", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(_batalla)!;
        for(var id = 1; id <= 2; id++)
        {
            var usada = new Skill($"USADA{id}", $"Skill {id}", "Skill ya lanzada en la batalla de aura.", CategoriaSkill.Gestual, 2) { Id = id };
            _batalla.Jugador1.DeclararSkill(usada, 4, FechaActual);
            _batalla.Jugador2.DeclararSkill(usada, 4, FechaActual);
            historial.Add(new Lanzamiento(_batalla, (id * 2) - 1, _batalla.Jugador1, usada, FechaLanzamiento));
            historial.Add(new Lanzamiento(_batalla, id * 2, _batalla.Jugador2, usada, FechaLanzamiento));
            var obligatoria = new Skill($"OBLIG{id}", $"Obligatoria {id}", "Skill obligatoria del torneo de aura.", CategoriaSkill.Gestual, 2) { Id = id + 4 };
            _batalla.Jugador1.DeclararSkill(obligatoria, 4, FechaActual);
            _batalla.Torneo.AsociarSkillObligatoria(obligatoria);
        }

        Assert.ThrowsExactly<SkillObligatoriaPendienteException>(
            () => _servicio.Lanzar(12, 7, new LanzamientoRequest { SkillId = 3 }));

        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.HasCount(4, _batalla.Lanzamientos);
        Assert.AreEqual(7, _batalla.JugadorTurnoId);
    }

    private void ConfigurarLanzamientoValido()
    {
        _batallas.Setup(r => r.ObtenerPorId(12)).Returns(_batalla);
        _usuarios.Setup(r => r.ObtenerConRepertorio(7)).Returns(_batalla.Jugador1);
        _skills.Setup(r => r.ObtenerPorId(3)).Returns(_skill);
        _reloj.Setup(r => r.GetUtcNow()).Returns(FechaLanzamiento);
        _batallas.Setup(r => r.Guardar());
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual) { Id = id };
    }
}
