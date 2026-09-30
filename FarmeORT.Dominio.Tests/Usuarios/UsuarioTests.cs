using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

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

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_NombreVacio_LanzaDatosInvalidos(string nombre)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(nombre: nombre));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Pe")]
    [DataRow("ApellidoDeVeintiseisLetras")]
    public void Constructor_ApellidoFueraDeRango_LanzaDatosInvalidos(string apellido)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(apellido: apellido));
    }

    private static Usuario CrearUsuario(string nombre = "Ana", string apellido = "Pérez")
    {
        return new Usuario(nombre, apellido, "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-ana");
    }
}
