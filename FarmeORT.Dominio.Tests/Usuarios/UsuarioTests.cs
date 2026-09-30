using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Usuarios;

[TestClass]
public sealed class UsuarioTests
{
    private static readonly DateOnly FechaActual = new(2026, 9, 30);

    [TestMethod]
    public void Constructor_DatosValidos_ConservaLosDatosYEloInicial()
    {
        var fechaNacimiento = new DateOnly(2000, 1, 15);

        var usuario = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", fechaNacimiento, Rol.Jugador, "hash-ana", FechaActual);

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

    [TestMethod]
    [DataRow("")]
    [DataRow("no-es-un-email")]
    [DataRow("ana@")]
    [DataRow("Ana <ana@farmeort.com>")]
    public void Constructor_EmailInvalido_LanzaDatosInvalidos(string email)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(email: email));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("ab")]
    [DataRow("NicknameDeVeintiunoXX")]
    [DataRow("con espacio")]
    [DataRow("con-guion")]
    public void Constructor_NicknameInvalido_LanzaDatosInvalidos(string nickname)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(nickname: nickname));
    }

    [TestMethod]
    public void Constructor_MenorDe13Anios_LanzaDatosInvalidos()
    {
        var cumple13Maniana = FechaActual.AddYears(-13).AddDays(1);

        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(fechaNacimiento: cumple13Maniana));
    }

    private static Usuario CrearUsuario(
        string nombre = "Ana",
        string apellido = "Pérez",
        string email = "ana@farmeort.com",
        string nickname = "anaperez",
        DateOnly? fechaNacimiento = null)
    {
        var nacimiento = fechaNacimiento ?? new DateOnly(2000, 1, 15);
        return new Usuario(nombre, apellido, email, nickname, nacimiento, Rol.Jugador, "hash-ana", FechaActual);
    }
}
