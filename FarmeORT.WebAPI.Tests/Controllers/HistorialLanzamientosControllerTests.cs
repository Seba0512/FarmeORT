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
public sealed class HistorialLanzamientosControllerTests
{
    [TestMethod]
    public void Listar_DevuelveHistorialYUsaSesionAutenticada()
    {
        var servicio = new Mock<IServicioBatallas>(MockBehavior.Strict);
        IReadOnlyList<LanzamientoResponse> response = [new LanzamientoResponse { Turno = 1, JugadorId = 7, SkillId = 3 }];
        servicio.Setup(s => s.ListarLanzamientos(12, 7, Roles.Jugador)).Returns(response);
        var controller = new HistorialLanzamientosController(servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        controller.HttpContext.Items[ClavesContexto.SesionAutenticada] =
            new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = 7, Rol = Roles.Jugador };

        var resultado = controller.Listar(12) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(response, resultado.Value);
        servicio.Verify(s => s.ListarLanzamientos(12, 7, Roles.Jugador), Times.Once);
        var atributo = typeof(HistorialLanzamientosController).GetCustomAttributes(typeof(AutorizacionAttribute), inherit: true)
            .OfType<AutorizacionAttribute>().Single();
        CollectionAssert.AreEqual(new[] { Roles.Jugador, Roles.Organizador, Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }
}
