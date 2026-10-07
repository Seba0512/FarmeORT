using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioLongitudEmailTests
{
    [TestMethod]
    [DataRow("registro", 254, true)]
    [DataRow("registro", 255, false)]
    [DataRow("registro", 262, false)]
    [DataRow("alta", 254, true)]
    [DataRow("alta", 255, false)]
    [DataRow("alta", 262, false)]
    [DataRow("modificacion", 254, true)]
    [DataRow("modificacion", 255, false)]
    [DataRow("modificacion", 262, false)]
    public void Guardar_LongitudEmail_RechazaExcesoSinPersistir(string operacion, int longitud, bool permitido)
    {
        var email = new string('a', longitud - 12) + "@example.com";
        var fecha = new DateOnly(2026, 9, 30);
        var usuario = new Usuario("Ana", "Pérez", "ana@example.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-anterior", fecha)
        {
            Id = 5,
        };
        var request = new UsuarioAdminRequest
        {
            Nombre = "Carla",
            Apellido = "Gómez",
            Email = email,
            Nickname = "carlagomez",
            FechaNacimiento = new DateOnly(1998, 6, 3),
            Rol = Roles.Jugador,
            Contrasena = "ClaveSegura123!!",
        };
        var repositorio = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        repositorio.Setup(r => r.ObtenerPorEmail(email)).Returns((Usuario?)null);
        repositorio.Setup(r => r.ExistePorNickname(request.Nickname)).Returns(false);
        repositorio.Setup(r => r.ObtenerPorId(5)).Returns(usuario);
        repositorio.Setup(r => r.Agregar(It.Is<Usuario>(u => u.Email == email)));
        repositorio.Setup(r => r.Actualizar(usuario));
        var hasheador = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        hasheador.Setup(h => h.Hashear(request.Contrasena)).Returns("hash-nuevo");
        var reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        var servicio = new ServicioUsuarios(repositorio.Object,
            new Mock<IRepositorioTorneos>(MockBehavior.Strict).Object,
            new Mock<IRepositorioBatallas>(MockBehavior.Strict).Object,
            new Mock<IRepositorioInscripciones>(MockBehavior.Strict).Object,
            hasheador.Object, reloj.Object);
        Func<UsuarioResponse> guardar = operacion switch
        {
            "registro" => () => servicio.RegistrarJugador(request),
            "alta" => () => servicio.Crear(request),
            _ => () => servicio.Modificar(5, request, 1),
        };

        if(permitido)
        {
            var response = guardar();

            Assert.AreEqual(email, response.Email);
            if(operacion == "modificacion")
            {
                repositorio.Verify(r => r.Actualizar(usuario), Times.Once);
            }
            else
            {
                repositorio.Verify(r => r.Agregar(It.Is<Usuario>(u => u.Email == email)), Times.Once);
            }
        }
        else
        {
            var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => guardar());

            Assert.AreEqual("El email no puede tener más de 254 caracteres.", excepcion.Message);
            repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
            repositorio.Verify(r => r.Actualizar(It.IsAny<Usuario>()), Times.Never);
            Assert.AreEqual("Ana", usuario.Nombre);
            Assert.AreEqual("Pérez", usuario.Apellido);
            Assert.AreEqual("ana@example.com", usuario.Email);
            Assert.AreEqual("anaperez", usuario.Nickname);
            Assert.AreEqual(new DateOnly(2000, 1, 15), usuario.FechaNacimiento);
            Assert.AreEqual(Rol.Jugador, usuario.Rol);
            Assert.AreEqual("hash-anterior", usuario.ContrasenaHash);
        }
    }
}
