using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Batallas;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class BatallasControllerTests
{
    [TestMethod]
    public void Consultas_RequierenAutenticacionYLosTresRoles()
    {
        var atributo = typeof(BatallasController).GetCustomAttributes(typeof(AutorizacionAttribute), inherit: true)
            .OfType<AutorizacionAttribute>().SingleOrDefault();

        Assert.IsNotNull(atributo);
        Assert.IsInstanceOfType<AutenticacionAttribute>(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Jugador, Roles.Organizador, Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }

    [TestMethod]
    public void ObtenerPorId_BatallaExistente_Devuelve200YUsaSesion()
    {
        var servicio = new Mock<IServicioBatallas>(MockBehavior.Strict);
        var response = new BatallaDetalleResponse { Id = 12, Estado = "Finalizada", GanadorId = 7, Comentario = "Victoria por abandono." };
        servicio.Setup(s => s.ObtenerPorId(12, 7, Roles.Jugador)).Returns(response);
        var controller = CrearController(servicio.Object);

        var resultado = controller.ObtenerPorId(12) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(response, resultado.Value);
        servicio.Verify(s => s.ObtenerPorId(12, 7, Roles.Jugador), Times.Once);
    }

    [TestMethod]
    public void Listar_FiltroValido_Devuelve200YUsaIdentidadYRolAutenticados()
    {
        var servicio = new Mock<IServicioBatallas>(MockBehavior.Strict);
        var filtro = new FiltroBatallasRequest { TorneoId = 5, Estado = "Pendiente", Ronda = 1, JugadorId = 7, Limit = 10, Offset = 2 };
        IReadOnlyList<BatallaResponse> response = [new BatallaResponse { Id = 12 }];
        servicio.Setup(s => s.Listar(filtro, 7, Roles.Jugador)).Returns(response);
        var controller = CrearController(servicio.Object);

        var resultado = controller.Listar(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(response, resultado.Value);
        servicio.Verify(s => s.Listar(filtro, 7, Roles.Jugador), Times.Once);
    }

    private static BatallasController CrearController(IServicioBatallas servicio)
    {
        var controller = new BatallasController(servicio)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        controller.HttpContext.Items[ClavesContexto.SesionAutenticada] =
            new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = 7, Rol = Roles.Jugador };
        return controller;
    }
}
