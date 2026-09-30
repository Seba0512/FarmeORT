using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Usuarios;
using FarmeORT.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class JugadoresControllerTests
{
    [TestMethod]
    public void Registrar_DatosValidos_Devuelve201ConLocationYJugador()
    {
        var request = new RegistroJugadorRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = "ana@farmeort.com",
            Nickname = "anaperez",
            Contrasena = "ClaveSegura123!!",
            FechaNacimiento = new DateOnly(2000, 1, 15),
        };
        var jugador = new UsuarioResponse { Id = 5, Rol = Roles.Jugador, Elo = 1200 };
        var servicio = new Mock<IServicioUsuarios>(MockBehavior.Strict);
        servicio.Setup(s => s.RegistrarJugador(request)).Returns(jugador);
        var controller = new JugadoresController(servicio.Object);

        var resultado = controller.Registrar(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/usuarios/5", resultado.Location);
        Assert.AreSame(jugador, resultado.Value);
        servicio.Verify(s => s.RegistrarJugador(request), Times.Once);
    }
}
