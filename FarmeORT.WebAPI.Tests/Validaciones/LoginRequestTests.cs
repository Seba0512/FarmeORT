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
}
