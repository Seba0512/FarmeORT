using System.Reflection;
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
    [DataRow(false)]
    [DataRow(true)]
    public void Listar_TorneoVisible_Devuelve200IndicandoSiConsultaUnAnonimo(bool autenticado)
    {
        AutenticarSi(autenticado);
        IReadOnlyList<RondaResponse> rondas = [new RondaResponse { Numero = 1 }];
        _servicio.Setup(s => s.Listar(TorneoId, !autenticado)).Returns(rondas);

        var resultado = _controller.Listar(TorneoId) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(rondas, resultado.Value);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ObtenerPorNumero_RondaExistente_Devuelve200IndicandoSiConsultaUnAnonimo(bool autenticado)
    {
        AutenticarSi(autenticado);
        var ronda = new RondaResponse { Numero = 2 };
        _servicio.Setup(s => s.ObtenerPorNumero(TorneoId, 2, !autenticado)).Returns(ronda);

        var resultado = _controller.ObtenerPorNumero(TorneoId, 2) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(ronda, resultado.Value);
    }

    [TestMethod]
    public void Generar_ContratoDeSeguridad_PermiteSoloOrganizadorYAdministrador()
    {
        var metodo = typeof(RondasController).GetMethod(nameof(RondasController.Generar))!;

        var atributo = metodo.GetCustomAttribute<AutorizacionAttribute>();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Organizador, Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }

    [TestMethod]
    public void Listar_ContratoDeSeguridad_UsaAutenticacionOpcional()
    {
        var metodo = typeof(RondasController).GetMethod(nameof(RondasController.Listar))!;

        var atributo = metodo.GetCustomAttribute<AutenticacionAttribute>();

        Assert.IsInstanceOfType<AutenticacionOpcionalAttribute>(atributo);
    }

    [TestMethod]
    public void ObtenerPorNumero_ContratoDeSeguridad_UsaAutenticacionOpcional()
    {
        var metodo = typeof(RondasController).GetMethod(nameof(RondasController.ObtenerPorNumero))!;

        var atributo = metodo.GetCustomAttribute<AutenticacionAttribute>();

        Assert.IsInstanceOfType<AutenticacionOpcionalAttribute>(atributo);
    }

    // Sin sesión en el contexto, el controller trata la consulta como anónima.
    private void AutenticarSi(bool autenticado)
    {
        if(autenticado)
        {
            Autenticar(OrganizadorId, Roles.Organizador);
        }
    }

    private void Autenticar(int usuarioId, string rol)
    {
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = usuarioId, Rol = rol };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
    }
}
