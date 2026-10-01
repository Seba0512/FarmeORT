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
