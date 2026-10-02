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
public sealed class FallosControllerTests
{
    [TestMethod]
    public void Fallar_DatosValidos_Devuelve201ConLocationYOrganizadorAutenticado()
    {
        var servicio = new Mock<IServicioFallos>(MockBehavior.Strict);
        var request = new FalloRequest { GanadorId = 7, Comentario = "Victoria por abandono." };
        var response = new BatallaDetalleResponse { Id = 12, Estado = "Finalizada" };
        servicio.Setup(s => s.Fallar(12, 2, request)).Returns(response);
        var controller = new FallosController(servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        controller.HttpContext.Items[ClavesContexto.SesionAutenticada] =
            new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = 2, Rol = Roles.Organizador };

        var resultado = controller.Fallar(12, request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/batallas/12", resultado.Location);
        Assert.AreSame(response, resultado.Value);
        servicio.Verify(s => s.Fallar(12, 2, request), Times.Once);
    }
}
