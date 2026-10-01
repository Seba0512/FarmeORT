using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Servicios.Torneos;
using Moq;

namespace FarmeORT.Servicios.Tests.Torneos;

[TestClass]
public sealed class ServicioSkillsObligatoriasTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;
    private const int SkillId = 3;

    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioSkills> _repositorioSkills = null!;
    private Torneo _torneo = null!;
    private Skill _skill = null!;
    private ServicioSkillsObligatorias _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioSkills = new Mock<IRepositorioSkills>(MockBehavior.Strict);
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = OrganizadorId,
        };
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 8, organizador, FechaActual)
        {
            Id = TorneoId,
        };
        _skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2)
        {
            Id = SkillId,
        };
        _servicio = new ServicioSkillsObligatorias(_repositorioTorneos.Object, _repositorioSkills.Object);
    }

    [TestMethod]
    public void Asociar_SkillActivaEnTorneoAbierto_LaGuardaComoObligatoriaYLaDevuelve()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioSkills.Setup(r => r.ObtenerPorId(SkillId)).Returns(_skill);
        _repositorioTorneos.Setup(r => r.Actualizar(_torneo));

        var response = _servicio.Asociar(TorneoId, new SkillObligatoriaRequest { SkillId = SkillId }, OrganizadorId);

        _repositorioTorneos.Verify(r => r.Actualizar(It.Is<Torneo>(t => t == _torneo && t.SkillsObligatorias.Single() == _skill)), Times.Once);
        Assert.AreEqual(SkillId, response.Id);
        Assert.AreEqual("SIXSEVEN", response.Codigo);
        Assert.AreEqual(CategoriasSkill.Gestual, response.Categoria);
        Assert.IsTrue(response.Activa);
    }

    [TestMethod]
    public void Asociar_TorneoInexistente_LanzaTorneoNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(99)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(
            () => _servicio.Asociar(99, new SkillObligatoriaRequest { SkillId = SkillId }, OrganizadorId));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Asociar_OrganizadorNoAsignado_LanzaNoEsOrganizadorDelTorneoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(
            () => _servicio.Asociar(TorneoId, new SkillObligatoriaRequest { SkillId = SkillId }, 99));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }
}
