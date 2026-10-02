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
public sealed class LanzamientosControllerTests
{
    [TestMethod]
    public void Lanzar_RequiereAutenticacionYRolJugador()
    {
        var atributo = typeof(LanzamientosController).GetCustomAttributes(typeof(AutorizacionAttribute), inherit: true)
            .OfType<AutorizacionAttribute>().SingleOrDefault();

        Assert.IsNotNull(atributo);
        Assert.IsInstanceOfType<AutenticacionAttribute>(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Jugador }, atributo.RolesPermitidos.ToArray());
    }

    [TestMethod]
    public void Lanzar_DatosValidos_Devuelve201ConLocationYUsaJugadorAutenticado()
    {
        var servicio = new Mock<IServicioLanzamientos>(MockBehavior.Strict);
        var request = new LanzamientoRequest { SkillId = 3 };
        var response = new LanzamientoResultadoResponse { Turno = 1, Estado = "Pendiente", JugadorTurnoId = 8 };
        servicio.Setup(s => s.Lanzar(12, 7, request)).Returns(response);
        var controller = new LanzamientosController(servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        controller.HttpContext.Items[ClavesContexto.SesionAutenticada] =
            new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = 7, Rol = Roles.Jugador };

        var resultado = controller.Lanzar(12, request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/batallas/12/lanzamientos", resultado.Location);
        Assert.AreSame(response, resultado.Value);
        servicio.Verify(s => s.Lanzar(12, 7, request), Times.Once);
    }
}
