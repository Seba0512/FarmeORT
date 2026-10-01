using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.IServicios.Torneos;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class SkillsObligatoriasControllerTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;

    private Mock<IServicioSkillsObligatorias> _servicio = null!;
    private SkillsObligatoriasController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new Mock<IServicioSkillsObligatorias>(MockBehavior.Strict);
        _controller = new SkillsObligatoriasController(_servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod]
    public void Asociar_SkillValida_Devuelve201ConLocationYLaSkillDelOrganizadorAutenticado()
    {
        AutenticarOrganizador();
        var request = new SkillObligatoriaRequest { SkillId = 3 };
        var skill = CrearSkill();
        _servicio.Setup(s => s.Asociar(TorneoId, request, OrganizadorId)).Returns(skill);

        var resultado = _controller.Asociar(TorneoId, request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/torneos/4/skills-obligatorias/3", resultado.Location);
        Assert.AreSame(skill, resultado.Value);
        _servicio.Verify(s => s.Asociar(TorneoId, request, OrganizadorId), Times.Once);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Listar_TorneoVisible_Devuelve200IndicandoSiConsultaUnAnonimo(bool autenticado)
    {
        if(autenticado)
        {
            AutenticarOrganizador();
        }

        IReadOnlyList<SkillResponse> skills = [CrearSkill()];
        _servicio.Setup(s => s.Listar(TorneoId, !autenticado)).Returns(skills);

        var resultado = _controller.Listar(TorneoId) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(skills, resultado.Value);
    }

    [TestMethod]
    public void Quitar_SkillAsociada_Devuelve204YLaQuitaConElOrganizadorAutenticado()
    {
        AutenticarOrganizador();
        _servicio.Setup(s => s.Quitar(TorneoId, 3, OrganizadorId));

        var resultado = _controller.Quitar(TorneoId, 3) as NoContentResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status204NoContent, resultado.StatusCode);
        _servicio.Verify(s => s.Quitar(TorneoId, 3, OrganizadorId), Times.Once);
    }

    private void AutenticarOrganizador()
    {
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = OrganizadorId, Rol = Roles.Organizador };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
    }

    private static SkillResponse CrearSkill()
    {
        return new SkillResponse
        {
            Id = 3,
            Codigo = "SIXSEVEN",
            Nombre = "Six-Seven",
            Descripcion = "Gesto alternando las manos hacia arriba y abajo.",
            Categoria = CategoriasSkill.Gestual,
            Dificultad = 2,
            Activa = true,
        };
    }
}
