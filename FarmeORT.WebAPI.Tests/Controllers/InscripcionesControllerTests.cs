using FarmeORT.DTOs.Inscripciones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Inscripciones;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class InscripcionesControllerTests
{
    private const int JugadorId = 7;

    private Mock<IServicioInscripciones> _servicio = null!;
    private InscripcionesController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new Mock<IServicioInscripciones>(MockBehavior.Strict);
        _controller = new InscripcionesController(_servicio.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod]
    public void Inscribir_TorneoValido_Devuelve201ConLocationYLaInscripcionDelJugadorAutenticado()
    {
        Autenticar(JugadorId, Roles.Jugador);
        var request = new InscripcionRequest { TorneoId = 4 };
        var inscripcion = CrearResponse();
        _servicio.Setup(s => s.Inscribir(JugadorId, request)).Returns(inscripcion);

        var resultado = _controller.Inscribir(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/inscripciones/12", resultado.Location);
        Assert.AreSame(inscripcion, resultado.Value);
        _servicio.Verify(s => s.Inscribir(JugadorId, request), Times.Once);
    }

    [TestMethod]
    public void Listar_ConFiltros_Devuelve200ConLasInscripcionesVisiblesParaElUsuarioAutenticado()
    {
        Autenticar(2, Roles.Organizador);
        var filtro = new FiltroInscripcionesRequest { TorneoId = 4, Estado = "Aceptada" };
        IReadOnlyList<InscripcionResponse> inscripciones = [CrearResponse()];
        _servicio.Setup(s => s.Listar(filtro, 2, Roles.Organizador)).Returns(inscripciones);

        var resultado = _controller.Listar(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(inscripciones, resultado.Value);
    }

    private void Autenticar(int usuarioId, string rol)
    {
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = usuarioId, Rol = rol };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
    }

    private static InscripcionResponse CrearResponse()
    {
        return new InscripcionResponse
        {
            Id = 12,
            JugadorId = JugadorId,
            Nickname = "anaperez",
            Elo = 1200,
            TorneoId = 4,
            Torneo = "Copa Aura",
            Estado = "Aceptada",
            Fecha = new DateTime(2026, 10, 1, 12, 0, 0),
        };
    }
}
