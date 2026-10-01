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
        Assert.AreEqual("/api/v1/jugadores/actual/repertorio/3", resultado.Location);
        Assert.AreSame(declarada, resultado.Value);
        _servicioRepertorio.Verify(s => s.Declarar(JugadorId, request), Times.Once);
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
