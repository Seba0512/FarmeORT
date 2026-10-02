using FarmeORT.DTOs.Rondas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Rondas;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class RondasControllerTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;

    private Mock<IServicioRondas> _servicio = null!;
    private RondasController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new Mock<IServicioRondas>(MockBehavior.Strict);
        _controller = new RondasController(_servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod]
    public void Generar_OrganizadorAutenticado_Devuelve201ConLocationYLaRonda()
    {
        Autenticar(OrganizadorId, Roles.Organizador);
        var ronda = new RondaResponse { Numero = 1 };
        _servicio.Setup(s => s.Generar(TorneoId, OrganizadorId, Roles.Organizador)).Returns(ronda);

        var resultado = _controller.Generar(TorneoId) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/torneos/4/rondas/1", resultado.Location);
        Assert.AreSame(ronda, resultado.Value);
    }

    [TestMethod]
    public void Generar_ContratoDeSeguridad_PermiteSoloOrganizadorYAdministrador()
    {
        var metodo = typeof(RondasController).GetMethod(nameof(RondasController.Generar));

        var atributo = metodo?.GetCustomAttributes(typeof(AutorizacionAttribute), inherit: true).OfType<AutorizacionAttribute>().SingleOrDefault();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEquivalent(new[] { Roles.Organizador, Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }

    private void Autenticar(int usuarioId, string rol)
    {
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = usuarioId, Rol = rol };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
    }
}
