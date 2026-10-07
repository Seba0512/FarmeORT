using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class RegistroJugadorRequestTests
{
    [TestMethod]
    [DataRow(1, 3, 3, 15)]
    [DataRow(50, 25, 20, 25)]
    public void Validar_LimitesPermitidos_EsValido(int largoNombre, int largoApellido, int largoNickname, int largoContrasena)
    {
        var request = CrearRequest(
            nombre: new string('A', largoNombre),
            apellido: new string('P', largoApellido),
            nickname: new string('n', largoNickname),
            contrasena: "Aa1!" + new string('a', largoContrasena - 4));
        var errores = new List<ValidationResult>();

        Assert.IsTrue(Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("NombreDeCincuentaYUnCaracteresParaProbarElLimiteXYZ")]
    public void Validar_NombreInvalido_EsInvalido(string nombre)
    {
        var request = CrearRequest(nombre: nombre);

        AssertCampoInvalido(request, nameof(RegistroJugadorRequest.Nombre));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Pe")]
    [DataRow("ApellidoDeVeintiseisLetras")]
    public void Validar_ApellidoInvalido_EsInvalido(string apellido)
    {
        var request = CrearRequest(apellido: apellido);

        AssertCampoInvalido(request, nameof(RegistroJugadorRequest.Apellido));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("no-es-un-email")]
    public void Validar_EmailInvalido_EsInvalido(string email)
    {
        var request = CrearRequest(email: email);

        AssertCampoInvalido(request, nameof(RegistroJugadorRequest.Email));
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

        AssertCampoInvalido(request, nameof(RegistroJugadorRequest.Nickname));
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

        AssertCampoInvalido(request, nameof(RegistroJugadorRequest.Contrasena));
    }

    [TestMethod]
    public void Validar_FechaNacimientoAusente_EsInvalida()
    {
        AssertCampoInvalido(new RegistroJugadorRequest(), nameof(RegistroJugadorRequest.FechaNacimiento));
    }

    [TestMethod]
    [DataRow(254, true)]
    [DataRow(255, false)]
    [DataRow(262, false)]
    public void Validar_LongitudEmail_RespetaCapacidadPersistente(int longitud, bool esperado)
    {
        var email = new string('a', longitud - 12) + "@example.com";
        var request = CrearRequest(email: email);
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.AreEqual(esperado, esValido);
        if(!esperado)
        {
            var error = errores.Single(e => e.MemberNames.Contains(nameof(request.Email)));
            Assert.AreEqual("El email no puede tener más de 254 caracteres.", error.ErrorMessage);
        }
    }

    private static RegistroJugadorRequest CrearRequest(
        string nombre = "Ana",
        string apellido = "Pérez",
        string email = "ana@farmeort.com",
        string nickname = "anaperez",
        string contrasena = "ClaveSegura123!!")
    {
        return new RegistroJugadorRequest
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Nickname = nickname,
            Contrasena = contrasena,
            FechaNacimiento = new DateOnly(2000, 1, 15),
        };
    }

    private static void AssertCampoInvalido(RegistroJugadorRequest request, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
