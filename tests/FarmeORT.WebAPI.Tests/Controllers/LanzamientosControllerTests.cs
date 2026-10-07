using System.Reflection;
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
    private const int BatallaId = 12;
    private const int JugadorId = 7;

    private Mock<IServicioLanzamientos> _servicioLanzamientos = null!;
    private Mock<IServicioBatallas> _servicioBatallas = null!;
    private LanzamientosController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioLanzamientos = new Mock<IServicioLanzamientos>(MockBehavior.Strict);
        _servicioBatallas = new Mock<IServicioBatallas>(MockBehavior.Strict);
        _controller = new LanzamientosController(_servicioLanzamientos.Object, _servicioBatallas.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] =
            new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = JugadorId, Rol = Roles.Jugador };
    }

    [TestMethod]
    public void Listar_Seguridad_ExigeRolJugadorOrganizadorOAdministrador()
    {
        var metodo = typeof(LanzamientosController).GetMethod(nameof(LanzamientosController.Listar))!;

        var atributo = metodo.GetCustomAttribute<AutorizacionAttribute>();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Jugador, Roles.Organizador, Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }

    [TestMethod]
    public void Listar_BatallaVisible_Devuelve200ConElHistorialYUsaLaSesionAutenticada()
    {
        IReadOnlyList<LanzamientoResponse> response = [new LanzamientoResponse { Turno = 1, JugadorId = JugadorId, SkillId = 3 }];
        _servicioBatallas.Setup(s => s.ListarLanzamientos(BatallaId, JugadorId, Roles.Jugador)).Returns(response);

        var resultado = _controller.Listar(BatallaId) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(response, resultado.Value);
        _servicioBatallas.Verify(s => s.ListarLanzamientos(BatallaId, JugadorId, Roles.Jugador), Times.Once);
    }

    [TestMethod]
    public void Lanzar_Seguridad_ExigeRolJugador()
    {
        var metodo = typeof(LanzamientosController).GetMethod(nameof(LanzamientosController.Lanzar))!;

        var atributo = metodo.GetCustomAttribute<AutorizacionAttribute>();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Jugador }, atributo.RolesPermitidos.ToArray());
    }

    [TestMethod]
    public void Lanzar_DatosValidos_Devuelve201ConLocationYUsaJugadorAutenticado()
    {
        var request = new LanzamientoRequest { SkillId = 3 };
        var response = new LanzamientoResultadoResponse { Turno = 1, Estado = "Pendiente", JugadorTurnoId = 8 };
        _servicioLanzamientos.Setup(s => s.Lanzar(BatallaId, JugadorId, request)).Returns(response);

        var resultado = _controller.Lanzar(BatallaId, request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/batallas/12/lanzamientos", resultado.Location);
        Assert.AreSame(response, resultado.Value);
        _servicioLanzamientos.Verify(s => s.Lanzar(BatallaId, JugadorId, request), Times.Once);
    }
}
