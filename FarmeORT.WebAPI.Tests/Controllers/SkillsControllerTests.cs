using FarmeORT.DTOs.Skills;
using FarmeORT.IServicios.Skills;
using FarmeORT.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class SkillsControllerTests
{
    private Mock<IServicioSkills> _servicioSkills = null!;
    private SkillsController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSkills = new Mock<IServicioSkills>(MockBehavior.Strict);
        _controller = new SkillsController(_servicioSkills.Object);
    }

    [TestMethod]
    public void Crear_DatosValidos_Devuelve201ConLocationYSkill()
    {
        var request = new SkillRequest
        {
            Codigo = "SIXSEVEN",
            Nombre = "Six-Seven",
            Descripcion = "Gesto alternando las manos hacia arriba y abajo.",
            Categoria = "Gestual",
            Dificultad = 2,
        };
        var skill = new SkillResponse
        {
            Id = 3,
            Codigo = "SIXSEVEN",
            Nombre = "Six-Seven",
            Descripcion = "Gesto alternando las manos hacia arriba y abajo.",
            Categoria = "Gestual",
            Dificultad = 2,
            Activa = true,
        };
        _servicioSkills.Setup(s => s.Crear(request)).Returns(skill);

        var resultado = _controller.Crear(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/skills/3", resultado.Location);
        Assert.AreSame(skill, resultado.Value);
        _servicioSkills.Verify(s => s.Crear(request), Times.Once);
    }

    [TestMethod]
    public void ObtenerPorId_SkillExistente_Devuelve200ConLaSkill()
    {
        var skill = new SkillResponse { Id = 3, Codigo = "SIXSEVEN", Nombre = "Six-Seven", Categoria = "Gestual", Dificultad = 2, Activa = true };
        _servicioSkills.Setup(s => s.ObtenerPorId(3)).Returns(skill);

        var resultado = _controller.ObtenerPorId(3) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(skill, resultado.Value);
    }

    [TestMethod]
    public void Listar_ConFiltros_Devuelve200ConLasSkills()
    {
        var filtro = new FiltroSkillsRequest { Categoria = "Gestual", Activa = true, Limit = 10, Offset = 0 };
        IReadOnlyList<SkillResponse> skills = [new SkillResponse { Id = 3, Nombre = "Six-Seven" }];
        _servicioSkills.Setup(s => s.Listar(filtro)).Returns(skills);

        var resultado = _controller.Listar(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(skills, resultado.Value);
    }

    [TestMethod]
    public void Modificar_DatosValidos_Devuelve200ConLaSkillModificada()
    {
        var request = new SkillRequest
        {
            Codigo = "SIXSEVEN",
            Nombre = "Six-Seven Pro",
            Descripcion = "Gesto alternando las manos hacia arriba y abajo.",
            Categoria = "Gestual",
            Dificultad = 3,
        };
        var skill = new SkillResponse { Id = 3, Codigo = "SIXSEVEN", Nombre = "Six-Seven Pro", Categoria = "Gestual", Dificultad = 3, Activa = true };
        _servicioSkills.Setup(s => s.Modificar(3, request)).Returns(skill);

        var resultado = _controller.Modificar(3, request) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(skill, resultado.Value);
        _servicioSkills.Verify(s => s.Modificar(3, request), Times.Once);
    }

    [TestMethod]
    public void CambiarEstado_Desactivar_Devuelve200ConLaSkillActualizada()
    {
        var request = new SkillEstadoRequest { Activa = false };
        var skill = new SkillResponse { Id = 3, Codigo = "SIXSEVEN", Nombre = "Six-Seven", Categoria = "Gestual", Dificultad = 2, Activa = false };
        _servicioSkills.Setup(s => s.CambiarEstado(3, request)).Returns(skill);

        var resultado = _controller.CambiarEstado(3, request) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(skill, resultado.Value);
        _servicioSkills.Verify(s => s.CambiarEstado(3, request), Times.Once);
    }
}
