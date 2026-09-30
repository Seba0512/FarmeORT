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
