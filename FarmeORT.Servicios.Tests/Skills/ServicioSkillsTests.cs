using FarmeORT.Dominio.Skills;
using FarmeORT.DTOs.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.Servicios.Skills;
using Moq;

namespace FarmeORT.Servicios.Tests.Skills;

[TestClass]
public sealed class ServicioSkillsTests
{
    private const string Codigo = "SIXSEVEN";
    private const string Nombre = "Six-Seven";
    private const string Descripcion = "Gesto alternando las manos hacia arriba y abajo.";

    private Mock<IRepositorioSkills> _repositorioSkills = null!;
    private ServicioSkills _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioSkills = new Mock<IRepositorioSkills>(MockBehavior.Strict);
        _servicio = new ServicioSkills(_repositorioSkills.Object);
    }

    [TestMethod]
    public void Crear_DatosValidos_GuardaLaSkillActiva()
    {
        ConfigurarAltaValida();

        _servicio.Crear(CrearRequest());

        _repositorioSkills.Verify(
            r => r.Agregar(It.Is<Skill>(s =>
                s.Codigo == Codigo
                && s.Nombre == Nombre
                && s.Descripcion == Descripcion
                && s.Categoria == CategoriaSkill.Gestual
                && s.Dificultad == 2
                && s.Activa)),
            Times.Once);
    }

    [TestMethod]
    public void Crear_DatosValidos_DevuelveLaSkillCreada()
    {
        ConfigurarAltaValida();

        var response = _servicio.Crear(CrearRequest());

        Assert.AreEqual(Codigo, response.Codigo);
        Assert.AreEqual(Nombre, response.Nombre);
        Assert.AreEqual(Descripcion, response.Descripcion);
        Assert.AreEqual(CategoriasSkill.Gestual, response.Categoria);
        Assert.AreEqual(2, response.Dificultad);
        Assert.IsTrue(response.Activa);
    }

    [TestMethod]
    public void Crear_CodigoExistente_LanzaCodigoSkillYaExisteSinGuardar()
    {
        _repositorioSkills.Setup(r => r.ExistePorCodigo(Codigo)).Returns(true);

        Assert.ThrowsExactly<CodigoSkillYaExisteException>(() => _servicio.Crear(CrearRequest()));
        _repositorioSkills.Verify(r => r.Agregar(It.IsAny<Skill>()), Times.Never);
    }

    [TestMethod]
    [DataRow("Cocina")]
    [DataRow("1")]
    public void Crear_CategoriaInexistente_LanzaDatosInvalidosSinConsultarLaBase(string categoria)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Crear(CrearRequest(categoria: categoria)));

        Assert.AreEqual("La categoría debe ser Gestual, Vocal, Baile o Meme.", excepcion.Message);
    }

    [TestMethod]
    public void Crear_DificultadOmitida_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Crear(CrearRequest(dificultad: null)));

        Assert.AreEqual("La dificultad es obligatoria.", excepcion.Message);
    }

    [TestMethod]
    public void ObtenerPorId_SkillExistente_DevuelveSusDatos()
    {
        var skill = new Skill(Codigo, Nombre, Descripcion, CategoriaSkill.Vocal, 4) { Id = 3 };
        _repositorioSkills.Setup(r => r.ObtenerPorId(3)).Returns(skill);

        var response = _servicio.ObtenerPorId(3);

        Assert.AreEqual(3, response.Id);
        Assert.AreEqual(Codigo, response.Codigo);
        Assert.AreEqual(Nombre, response.Nombre);
        Assert.AreEqual(Descripcion, response.Descripcion);
        Assert.AreEqual(CategoriasSkill.Vocal, response.Categoria);
        Assert.AreEqual(4, response.Dificultad);
        Assert.IsTrue(response.Activa);
    }

    [TestMethod]
    public void ObtenerPorId_SkillInexistente_LanzaSkillNoEncontrada()
    {
        _repositorioSkills.Setup(r => r.ObtenerPorId(99)).Returns((Skill?)null);

        Assert.ThrowsExactly<SkillNoEncontradaException>(() => _servicio.ObtenerPorId(99));
    }

    [TestMethod]
    public void Listar_ConFiltros_DevuelveLasSkillsDelRepositorio()
    {
        var skill = new Skill(Codigo, Nombre, Descripcion, CategoriaSkill.Gestual, 2) { Id = 3 };
        _repositorioSkills.Setup(r => r.Listar(new FiltroSkills(CategoriaSkill.Gestual, 2, true, "six", 10, 5))).Returns([skill]);
        var filtro = new FiltroSkillsRequest
        {
            Categoria = CategoriasSkill.Gestual,
            Dificultad = 2,
            Activa = true,
            Nombre = "six",
            Limit = 10,
            Offset = 5,
        };

        var skills = _servicio.Listar(filtro);

        Assert.HasCount(1, skills);
        Assert.AreEqual(3, skills[0].Id);
        Assert.AreEqual(Codigo, skills[0].Codigo);
        Assert.AreEqual(CategoriasSkill.Gestual, skills[0].Categoria);
    }

    [TestMethod]
    public void Listar_SinFiltros_NoFiltraYUsaLaPaginacionPorDefecto()
    {
        _repositorioSkills.Setup(r => r.Listar(new FiltroSkills(null, null, null, null, 20, 0))).Returns([]);

        var skills = _servicio.Listar(new FiltroSkillsRequest());

        Assert.IsEmpty(skills);
    }

    private void ConfigurarAltaValida()
    {
        _repositorioSkills.Setup(r => r.ExistePorCodigo(Codigo)).Returns(false);
        _repositorioSkills.Setup(r => r.Agregar(It.Is<Skill>(s => s.Codigo == Codigo)));
    }

    private static SkillRequest CrearRequest(
        string codigo = Codigo,
        string categoria = CategoriasSkill.Gestual,
        int? dificultad = 2)
    {
        return new SkillRequest
        {
            Codigo = codigo,
            Nombre = Nombre,
            Descripcion = Descripcion,
            Categoria = categoria,
            Dificultad = dificultad,
        };
    }
}
