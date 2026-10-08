using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Sesiones;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class LoginRequestTests
{
    [TestMethod]
    [DataRow("", "Clave123!")]
    [DataRow("no-es-un-email", "Clave123!")]
    [DataRow("jugador@farmeort.com", "")]
    public void Validar_DatosInvalidos_EsInvalido(string email, string contrasena)
    {
        var request = new LoginRequest { Email = email, Contrasena = contrasena };
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
    }

    // Los detalles del 400 los ve el usuario: van en español, con las mismas frases que el registro.
    [TestMethod]
    [DataRow("", "Clave123!", "El email es obligatorio.")]
    [DataRow("no-es-un-email", "Clave123!", "El email no tiene un formato válido.")]
    [DataRow("jugador@farmeort.com", "", "La contraseña es obligatoria.")]
    public void Validar_DatosInvalidos_ExplicaElErrorEnEspanol(string email, string contrasena, string mensaje)
    {
        var request = new LoginRequest { Email = email, Contrasena = contrasena };
        var errores = new List<ValidationResult>();

        Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        CollectionAssert.Contains(errores.Select(e => e.ErrorMessage).ToList(), mensaje);
    }
}
