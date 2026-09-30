using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Tests.Usuarios;

[TestClass]
public sealed class UsuarioTests
{
    [TestMethod]
    public void Constructor_DatosValidos_ConservaLosDatosYEloInicial()
    {
        var fechaNacimiento = new DateOnly(2000, 1, 15);

        var usuario = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", fechaNacimiento, Rol.Jugador, "hash-ana");

        Assert.AreEqual("Ana", usuario.Nombre);
        Assert.AreEqual("Pérez", usuario.Apellido);
        Assert.AreEqual("ana@farmeort.com", usuario.Email);
        Assert.AreEqual("anaperez", usuario.Nickname);
        Assert.AreEqual(fechaNacimiento, usuario.FechaNacimiento);
        Assert.AreEqual(Rol.Jugador, usuario.Rol);
        Assert.AreEqual("hash-ana", usuario.ContrasenaHash);
        Assert.AreEqual(1200, usuario.Elo);
    }
}
