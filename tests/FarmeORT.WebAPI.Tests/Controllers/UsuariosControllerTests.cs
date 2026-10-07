using System.Reflection;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
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
    public void Modificar_DatosValidos_Devuelve200ConElUsuarioModificadoPorElAdministradorActual()
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
        var usuario = new UsuarioResponse { Id = 5, Nombre = "Ana", Email = "ana@farmeort.com", Rol = Roles.Organizador };
        AutenticarAdministrador(1);
        _servicioUsuarios.Setup(s => s.Modificar(5, request, 1)).Returns(usuario);

        var resultado = _controller.Modificar(5, request) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(usuario, resultado.Value);
        _servicioUsuarios.Verify(s => s.Modificar(5, request, 1), Times.Once);
    }

    [TestMethod]
    public void Eliminar_UsuarioExistente_Devuelve204YLoEliminaComoAdministradorActual()
    {
        AutenticarAdministrador(1);
        _servicioUsuarios.Setup(s => s.Eliminar(5, 1));

        var resultado = _controller.Eliminar(5);

        Assert.IsInstanceOfType<NoContentResult>(resultado);
        _servicioUsuarios.Verify(s => s.Eliminar(5, 1), Times.Once);
    }

    [TestMethod]
    public void UsuariosController_TodosLosEndpoints_RequierenSoloRolAdministrador()
    {
        var controller = typeof(UsuariosController);

        var atributo = controller.GetCustomAttribute<AutorizacionAttribute>();

        Assert.IsNotNull(atributo);
        CollectionAssert.AreEqual(new[] { Roles.Administrador }, atributo.RolesPermitidos.ToArray());
    }

    private void AutenticarAdministrador(int usuarioId)
    {
        var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = usuarioId, Rol = Roles.Administrador };
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
    }
}
