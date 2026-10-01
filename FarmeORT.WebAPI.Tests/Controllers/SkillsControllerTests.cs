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
}
