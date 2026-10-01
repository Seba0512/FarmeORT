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
public sealed class TorneosControllerTests
{
    private Mock<IServicioTorneos> _servicioTorneos = null!;
    private TorneosController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioTorneos = new Mock<IServicioTorneos>(MockBehavior.Strict);
        _controller = new TorneosController(_servicioTorneos.Object);
    }

    [TestMethod]
    public void Crear_DatosValidos_Devuelve201ConLocationYTorneo()
    {
        var request = CrearRequest();
        var torneo = CrearResponse();
        _servicioTorneos.Setup(s => s.Crear(request)).Returns(torneo);

        var resultado = _controller.Crear(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/torneos/4", resultado.Location);
        Assert.AreSame(torneo, resultado.Value);
        _servicioTorneos.Verify(s => s.Crear(request), Times.Once);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ObtenerPorId_TorneoVisible_Devuelve200IndicandoSiConsultaUnAnonimo(bool autenticado)
    {
        var torneo = CrearResponse();
        PrepararContexto(autenticado);
        _servicioTorneos.Setup(s => s.ObtenerPorId(4, !autenticado)).Returns(torneo);

        var resultado = _controller.ObtenerPorId(4) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(torneo, resultado.Value);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Listar_ConFiltros_Devuelve200IndicandoSiConsultaUnAnonimo(bool autenticado)
    {
        var filtro = new FiltroTorneosRequest { Estado = "Abierto", Nombre = "copa", Limit = 10, Offset = 0 };
        IReadOnlyList<TorneoResponse> torneos = [CrearResponse()];
        PrepararContexto(autenticado);
        _servicioTorneos.Setup(s => s.Listar(filtro, !autenticado)).Returns(torneos);

        var resultado = _controller.Listar(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(torneos, resultado.Value);
    }

    private void PrepararContexto(bool autenticado)
    {
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        if(autenticado)
        {
            var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = 7, Rol = Roles.Jugador };
            _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        }
    }

    private static TorneoRequest CrearRequest()
    {
        return new TorneoRequest
        {
            Codigo = "COPAAURA",
            Nombre = "Copa Aura",
            Descripcion = "Torneo eliminatorio de batallas de aura.",
            FechaInicio = new DateOnly(2026, 11, 1),
            FechaFin = new DateOnly(2026, 11, 30),
            Cupo = 8,
            OrganizadorId = 2,
        };
    }

    private static TorneoResponse CrearResponse()
    {
        return new TorneoResponse
        {
            Id = 4,
            Codigo = "COPAAURA",
            Nombre = "Copa Aura",
            Descripcion = "Torneo eliminatorio de batallas de aura.",
            Estado = "Abierto",
            FechaInicio = new DateOnly(2026, 11, 1),
            FechaFin = new DateOnly(2026, 11, 30),
            Cupo = 8,
            OrganizadorId = 2,
            Organizador = "Laura Gomez",
        };
    }
}
