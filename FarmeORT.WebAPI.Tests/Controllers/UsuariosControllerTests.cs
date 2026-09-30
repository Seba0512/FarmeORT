using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Usuarios;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class UsuariosControllerTests
{
    private Mock<IServicioUsuarios> _servicioUsuarios = null!;
    private UsuariosController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioUsuarios = new Mock<IServicioUsuarios>(MockBehavior.Strict);
        _controller = new UsuariosController(_servicioUsuarios.Object);
    }

    [TestMethod]
    public void Crear_DatosValidos_Devuelve201ConLocationYUsuario()
    {
        var request = new UsuarioAdminRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = "ana@farmeort.com",
            Nickname = "anaperez",
            Contrasena = "ClaveSegura123!!",
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Rol = Roles.Organizador,
        };
        var usuario = new UsuarioResponse
        {
            Id = 5,
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = "ana@farmeort.com",
            Nickname = "anaperez",
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Rol = Roles.Organizador,
        };
        _servicioUsuarios.Setup(s => s.Crear(request)).Returns(usuario);

        var resultado = _controller.Crear(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/usuarios/5", resultado.Location);
        Assert.AreSame(usuario, resultado.Value);
        _servicioUsuarios.Verify(s => s.Crear(request), Times.Once);
    }

    [TestMethod]
    public void ObtenerPorId_UsuarioExistente_Devuelve200ConElUsuario()
    {
        var usuario = new UsuarioResponse { Id = 5, Nombre = "Ana", Email = "ana@farmeort.com", Rol = Roles.Organizador };
        _servicioUsuarios.Setup(s => s.ObtenerPorId(5)).Returns(usuario);

        var resultado = _controller.ObtenerPorId(5) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(usuario, resultado.Value);
    }

    [TestMethod]
    public void Listar_ConFiltros_Devuelve200ConLosUsuarios()
    {
        var filtro = new FiltroUsuariosRequest { Nombre = "ana", Limit = 10, Offset = 0 };
        IReadOnlyList<UsuarioResponse> usuarios = [new UsuarioResponse { Id = 5, Nombre = "Ana" }];
        _servicioUsuarios.Setup(s => s.Listar(filtro)).Returns(usuarios);

        var resultado = _controller.Listar(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(usuarios, resultado.Value);
    }

    [TestMethod]
    public void UsuariosController_TodosLosEndpoints_RequierenRolAdministrador()
    {
        var atributo = typeof(UsuariosController)
            .GetCustomAttributes(typeof(AutorizacionAttribute), inherit: true)
            .OfType<AutorizacionAttribute>()
            .SingleOrDefault();

        Assert.IsNotNull(atributo);
        Assert.Contains(Roles.Administrador, atributo.RolesPermitidos);
    }
}
