using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class UsuarioAdminRequestTests
{
    [TestMethod]
    [DataRow("A", "Pérez", "anaperez", "ClaveSegura123!!", Roles.Organizador)]
    [DataRow("Ana", "Paz", "abc", "ClaveSegura12!!", Roles.Jugador)]
    [DataRow("Ana", "ApellidoDeVeinticincoLetr", "NicknameDeVeinteXXXX", "ClaveMuySegura12345!!Abcd", Roles.Administrador)]
    public void Validar_DatosValidos_EsValido(string nombre, string apellido, string nickname, string contrasena, string rol)
    {
        var request = CrearRequest(nombre: nombre, apellido: apellido, nickname: nickname, contrasena: contrasena, rol: rol);
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsTrue(esValido, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }

    [TestMethod]
    [DataRow("")]
    public void Validar_NombreInvalido_EsInvalido(string nombre)
    {
        var request = CrearRequest(nombre: nombre);

        AssertCampoInvalido(request, nameof(UsuarioAdminRequest.Nombre));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Pe")]
    [DataRow("ApellidoDeVeintiseisLetras")]
    public void Validar_ApellidoInvalido_EsInvalido(string apellido)
    {
        var request = CrearRequest(apellido: apellido);

        AssertCampoInvalido(request, nameof(UsuarioAdminRequest.Apellido));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("no-es-un-email")]
    public void Validar_EmailInvalido_EsInvalido(string email)
    {
        var request = CrearRequest(email: email);

        AssertCampoInvalido(request, nameof(UsuarioAdminRequest.Email));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("ab")]
    [DataRow("NicknameDeVeintiunoXX")]
    [DataRow("con espacio")]
    [DataRow("con-guion")]
    public void Validar_NicknameInvalido_EsInvalido(string nickname)
    {
        var request = CrearRequest(nickname: nickname);

        AssertCampoInvalido(request, nameof(UsuarioAdminRequest.Nickname));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Corta123!")]
    [DataRow("ClaveDemasiadoLarga12345!!")]
    [DataRow("sinmayusculas123!!")]
    [DataRow("SINMINUSCULAS123!!")]
    [DataRow("SinNumerosPeroLarga!!")]
    [DataRow("SinSimbolos12345Ab")]
    public void Validar_ContrasenaInvalida_EsInvalida(string contrasena)
    {
        var request = CrearRequest(contrasena: contrasena);

        AssertCampoInvalido(request, nameof(UsuarioAdminRequest.Contrasena));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Superusuario")]
    public void Validar_RolInvalido_EsInvalido(string rol)
    {
        var request = CrearRequest(rol: rol);

        AssertCampoInvalido(request, nameof(UsuarioAdminRequest.Rol));
    }

    private static UsuarioAdminRequest CrearRequest(
        string nombre = "Ana",
        string apellido = "Pérez",
        string email = "ana@farmeort.com",
        string nickname = "anaperez",
        string contrasena = "ClaveSegura123!!",
        string rol = Roles.Organizador)
    {
        return new UsuarioAdminRequest
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Nickname = nickname,
            Contrasena = contrasena,
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Rol = rol,
        };
    }

    private static void AssertCampoInvalido(UsuarioAdminRequest request, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
