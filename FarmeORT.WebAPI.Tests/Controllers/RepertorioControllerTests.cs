using System.Reflection;
using FarmeORT.DTOs.Repertorio;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Repertorio;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class RepertorioControllerTests
{
    private const int JugadorId = 7;

    private Mock<IServicioRepertorio> _servicioRepertorio = null!;
    private RepertorioController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioRepertorio = new Mock<IServicioRepertorio>(MockBehavior.Strict);
        _controller = new RepertorioController(_servicioRepertorio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = JugadorId, Rol = Roles.Jugador };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
    }

    [TestMethod]
    public void Declarar_DatosValidos_Devuelve201ConLocationYLaSkillDeclaradaDelJugadorAutenticado()
    {
        var request = new SkillDeclaradaRequest { SkillId = 3, Nivel = 4 };
        var declarada = CrearResponse();
        _servicioRepertorio.Setup(s => s.Declarar(JugadorId, request)).Returns(declarada);

        var resultado = _controller.Declarar(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/repertorios/3", resultado.Location);
        Assert.AreSame(declarada, resultado.Value);
        _servicioRepertorio.Verify(s => s.Declarar(JugadorId, request), Times.Once);
    }

    [TestMethod]
    public void Listar_JugadorAutenticado_Devuelve200ConSuRepertorio()
    {
        IReadOnlyList<SkillDeclaradaResponse> repertorio = [CrearResponse()];
        _servicioRepertorio.Setup(s => s.Listar(JugadorId)).Returns(repertorio);

        var resultado = _controller.Listar() as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(repertorio, resultado.Value);
    }

    [TestMethod]
    public void Quitar_SkillDeclarada_Devuelve204YLaQuitaDelJugadorAutenticado()
    {
        _servicioRepertorio.Setup(s => s.Quitar(JugadorId, 3));

        var resultado = _controller.Quitar(3) as NoContentResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status204NoContent, resultado.StatusCode);
        _servicioRepertorio.Verify(s => s.Quitar(JugadorId, 3), Times.Once);
    }

    [TestMethod]
    public void Controller_Repertorio_RequiereRolJugador()
    {
        var controller = typeof(RepertorioController);

        var atributo = controller.GetCustomAttribute<AutorizacionAttribute>();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Jugador }, atributo.RolesPermitidos.ToArray());
    }

    // El jugador sale del token: la ruta no lleva "actual" y queda en un solo nivel.
    [TestMethod]
    public void Controller_Ruta_EsRepertoriosSinElJugadorEnLaRuta()
    {
        var controller = typeof(RepertorioController);

        var ruta = controller.GetCustomAttribute<RouteAttribute>();

        Assert.IsNotNull(ruta);
        Assert.AreEqual("api/v1/repertorios", ruta.Template);
    }

    private static SkillDeclaradaResponse CrearResponse()
    {
        return new SkillDeclaradaResponse
        {
            SkillId = 3,
            Codigo = "SIXSEVEN",
            Nombre = "Six-Seven",
            Categoria = "Gestual",
            Activa = true,
            Nivel = 4,
            FechaDeclaracion = new DateOnly(2026, 10, 1),
        };
    }
}
