using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Repertorio;
using FarmeORT.DTOs.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Repertorio;
using Moq;

namespace FarmeORT.Servicios.Tests.Repertorio;

[TestClass]
public sealed class ServicioRepertorioTests
{
    private const int JugadorId = 7;
    private const int SkillId = 3;

    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IRepositorioSkills> _repositorioSkills = null!;
    private Mock<TimeProvider> _reloj = null!;
    private Usuario _jugador = null!;
    private Skill _skill = null!;
    private ServicioRepertorio _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioSkills = new Mock<IRepositorioSkills>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _jugador = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = JugadorId,
        };
        _skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2)
        {
            Id = SkillId,
        };
        _servicio = new ServicioRepertorio(_repositorioUsuarios.Object, _repositorioSkills.Object, _reloj.Object);
    }

    [TestMethod]
    public void Declarar_SkillActivaNoDeclarada_LaGuardaEnElRepertorioDelJugador()
    {
        ConfigurarDeclaracionValida();

        _servicio.Declarar(JugadorId, new SkillDeclaradaRequest { SkillId = SkillId, Nivel = 4 });

        _repositorioUsuarios.Verify(
            r => r.GuardarRepertorio(It.Is<Usuario>(u =>
                u == _jugador
                && u.Repertorio.Count == 1
                && u.Repertorio.Single().Skill == _skill
                && u.Repertorio.Single().Nivel == 4
                && u.Repertorio.Single().FechaDeclaracion == FechaActual)),
            Times.Once);
    }

    [TestMethod]
    public void Declarar_SkillActivaNoDeclarada_DevuelveLaSkillDeclarada()
    {
        ConfigurarDeclaracionValida();

        var response = _servicio.Declarar(JugadorId, new SkillDeclaradaRequest { SkillId = SkillId, Nivel = 4 });

        Assert.AreEqual(SkillId, response.SkillId);
        Assert.AreEqual("SIXSEVEN", response.Codigo);
        Assert.AreEqual("Six-Seven", response.Nombre);
        Assert.AreEqual(CategoriasSkill.Gestual, response.Categoria);
        Assert.IsTrue(response.Activa);
        Assert.AreEqual(4, response.Nivel);
        Assert.AreEqual(FechaActual, response.FechaDeclaracion);
    }

    [TestMethod]
    public void Declarar_SkillInexistente_LanzaSkillNoEncontradaSinGuardar()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerConRepertorio(JugadorId)).Returns(_jugador);
        _repositorioSkills.Setup(r => r.ObtenerPorId(99)).Returns((Skill?)null);

        Assert.ThrowsExactly<SkillNoEncontradaException>(
            () => _servicio.Declarar(JugadorId, new SkillDeclaradaRequest { SkillId = 99, Nivel = 4 }));
        _repositorioUsuarios.Verify(r => r.GuardarRepertorio(It.IsAny<Usuario>()), Times.Never);
    }

    private void ConfigurarDeclaracionValida()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerConRepertorio(JugadorId)).Returns(_jugador);
        _repositorioSkills.Setup(r => r.ObtenerPorId(SkillId)).Returns(_skill);
        _repositorioUsuarios.Setup(r => r.GuardarRepertorio(_jugador));
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
    }
}
