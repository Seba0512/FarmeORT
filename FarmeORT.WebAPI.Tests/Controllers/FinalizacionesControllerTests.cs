using FarmeORT.DTOs.Finalizaciones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Finalizaciones;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class FinalizacionesControllerTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;

    private Mock<IServicioFinalizaciones> _servicio = null!;
    private FinalizacionesController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new Mock<IServicioFinalizaciones>(MockBehavior.Strict);
        _controller = new FinalizacionesController(_servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod]
    public void Finalizar_OrganizadorAutenticado_Devuelve200ConElCampeon()
    {
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = OrganizadorId, Rol = Roles.Organizador };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        var finalizacion = new FinalizacionResponse { TorneoId = TorneoId, Estado = "Finalizado" };
        _servicio.Setup(s => s.Finalizar(TorneoId, OrganizadorId, Roles.Organizador)).Returns(finalizacion);

        var resultado = _controller.Finalizar(TorneoId) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(finalizacion, resultado.Value);
    }

    [TestMethod]
    public void Finalizar_ContratoDeSeguridad_PermiteSoloOrganizadorYAdministrador()
    {
        var metodo = typeof(FinalizacionesController).GetMethod(nameof(FinalizacionesController.Finalizar));

        var atributo = metodo?.GetCustomAttributes(typeof(AutorizacionAttribute), inherit: true).OfType<AutorizacionAttribute>().SingleOrDefault();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEquivalent(new[] { Roles.Organizador, Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }
}
