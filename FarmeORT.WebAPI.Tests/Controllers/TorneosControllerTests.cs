using FarmeORT.DTOs.Torneos;
using FarmeORT.IServicios.Torneos;
using FarmeORT.WebAPI.Controllers;
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
